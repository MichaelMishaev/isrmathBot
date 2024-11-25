﻿using BL.Serives;
using BLL.Objects;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BLL.Services
{
    public class WhatsAppService
    {
        private readonly ChatGPTService _chatGPTService;
        private readonly ExerciseRepository _exerciseRepository;
        private readonly IConfiguration _configuration;
        // Inject ChatGPTService through the constructor
        public WhatsAppService(ChatGPTService chatGPTService, ExerciseRepository exerciseRepository, IConfiguration configuration, string level="")
        {
            _chatGPTService = chatGPTService;
            _exerciseRepository = exerciseRepository;
            _configuration = configuration;
        }

        public async Task<string> GetExercisesFromGPT(string example, int creatorUserId, string creatorRole, int classId, string instructions,string level="")
        {
            level =  string.IsNullOrEmpty(level) ? "Medium" : level;

            string gptQuery = "Please create 12 unique math exercises " + instructions + " using the **same mathematical operation and format** as the given example. Difficulty level: " + level + ". Example: " + example + ".\n\n" +

"**Requirements:**\n" +
"- Each generated exercise **must use the same operation** (e.g., multiplication, division, addition, subtraction) as the given example and must have the same level of complexity.\n" +
"- Ensure each exercise has **only two operands**, if that is the case for the example. Maintain similar operand sizes to ensure the difficulty level is consistent.\n" +
"- **Ensure that the exercises are different from each other and from the example provided.**\n" +
"- Use **random numbers** for operands. The exercises must not use sequential patterns like 1 × 1, 1 × 2, 1 × 3, etc.\n" +
"- Randomize the numbers in a way that no clear pattern (e.g., incremental, decremental) can be observed across the exercises.\n" +
"- Limit the numbers to reasonable ranges if necessary but ensure diversity and randomness.\n" +
"- Provide the correct answer as a numeric value without additional text (e.g., no 'x = ...').\n\n" +

"**Hints Requirements:**\n" +
"- For each exercise, provide a hint that explains how to solve the problem in a step-by-step manner suitable for an 8-10-year-old child.\n" +
"- Hints should be in simple Hebrew and should include common mistakes that children might make, explained in a friendly way.\n" +
"- Add playful encouragement and relatable errors to keep young students engaged.\n" +
"- **Do not provide the answer in the hint**. Focus only on explaining the method.\n" +
"- Make the hint short and clear (2-3 sentences maximum).\n" +
"- **Avoid using double quotation marks (\") inside the hints to prevent JSON formatting issues. Use single quotes (') if necessary.**\n\n" +

"**Example Hint:**\n" +
"- For instance, if the exercise is *12 × 6*:\n" +
"'🎉 תזכרו שאם קשה לחשב ישר, אפשר לחלק את ה-12 ל-10 ו-2, ואז לכפול כל חלק ב-6 ולהוסיף אותם בסוף 😊 כפלו תחילה 6 ב-2 ואז 6 ב-10, ואז חברו את התוצאות.'\n\n" +

"**Output Requirements:**\n" +
"- Your response **must be in a valid JSON array format**.\n" +
"- Example format:\n" +
"[\n" +
"  { \"exercise\": \"A × B\", \"answer\": C, \"hint\": \"Your concise hint here.\", \"DifficultyLevel\": \"Easy\" },\n" +
"  ...\n" +
"]\n" +
"- Ensure each object has a `DifficultyLevel` field with values: \"Easy\", \"Medium\", or \"Hard\".\n" +
"- **Return only the JSON array**, with no additional text or characters before or after it.\n" +
"- **Ensure all strings are properly escaped according to JSON standards, especially in the hints. Do not include unescaped special characters or quotation marks within the strings.**\n\n" +

"**Remember**:\n" +
"- Use lots of emojis to make the hints fun and engaging for kids 🎈🚀🖍️📚✨😊.\n" +
"- Do not add any special characters or text before or after the JSON array.\n" +
"- Return **only** the JSON array, and nothing else.\n";



            try
            {
                // Prepare two tasks for calling AskChatGPT
                var task1 = _chatGPTService.AskChatGPT(gptQuery);
                var task2 = _chatGPTService.AskChatGPT(gptQuery);

                // Run both tasks in parallel
                await Task.WhenAll(task1, task2);

                // Get the responses
                var response1 = await task1;
                var response2 = await task2;

                if (string.IsNullOrWhiteSpace(response1) || string.IsNullOrWhiteSpace(response2))
                {
                    return "שגיאה ביצירת תרגילים. אנא נסה שוב.";
                }

                // Process and deserialize each response
                List<ExerciseModel> exercises1 = await ProcessAssistantResponse(response1);
                List<ExerciseModel> exercises2 = await ProcessAssistantResponse(response2);

                // Combine the exercises, ensuring uniqueness
                var combinedExercises = exercises1.Concat(exercises2)
                                                 .GroupBy(e => e.Exercise)
                                                 .Select(g => g.First())
                                                 .ToList();

                // Serialize the combined exercises back to JSON
                string combinedResponse = JsonConvert.SerializeObject(combinedExercises);

                // Create a unique identifier for this pending exercise set
                string pendingId = Guid.NewGuid().ToString();

                // Save to temporary storage, deleting any existing pending exercises first
                await _exerciseRepository.SavePendingExercises(pendingId, combinedResponse, creatorUserId, creatorRole, classId);

                // Construct a message with the exercises and ask for confirmation
                var exercisesMessage = "התרגילים שנוצרו:\n" + string.Join("\n", combinedExercises.Select(e => e.Exercise)) +
                                       "\n\nהאם התרגילים מתאימים? כתוב 'כן' כדי לאשר ולשמור, או 'לא' כדי לספק הוראות חדשות.";

                return exercisesMessage;
            }
            catch (Exception e)
            {
                // Log the exception if necessary
                Console.WriteLine($"Error: {e.Message}");
                return $"שגיאה: {e.Message}";
            }

        }

        public async Task<string> GetHelpForStudent(string exercise)
        {
            string gptQuery = $@"
הסטודנט מתקשה בתרגיל: {exercise}. הסבר בקצרה את הדרך לפתרון בלי לתת את התשובה. השתמש במילים פשוטות, דוגמאות ברורות, ואימוג'ים 😊📘🔢 כדי להקל על ילדים בני 8 להבין. ההסבר חייב להיות קצר ומדויק, עד 30 מילים.

חשוב שהתשובה תהיה בפורמט הבא עבור תרגילי חיסור או חיבור בטור:
1. המספר הראשון בשורה נפרדת.
2. סימן המינוס ('-') או החיבור ('+') בשורה נפרדת.
3. המספר השני בשורה נפרדת לאחר הסימן.

דוגמה:
2,135
-
1,210

אם אתה מחלק את החישוב לחלקים, הסבר כל חלק אך **אל תיתן את התוצאה הסופית**.

תמיד פעל לפי הפורמט הזה:

🎉 זכרו שאפשר לחלק את {{X}} ל-{{Chunk1}} ו-{{Chunk2}}, ואז לכפול כל חלק ב-{{Multiplier}} ולהוסיף אותם בסוף 😊.
1. חשבו: {{Chunk1}} × {{Multiplier}} = {{IntermediateResult1}}.
2. חשבו: {{Chunk2}} × {{Multiplier}} = {{IntermediateResult2}}.
3. ציינו ששני החלקים צריכים להיאסף יחד.
";




            try
            {

                var response = await _chatGPTService.AskChatGPTReduceTokens(gptQuery);
                if (response == null)
                {
                    return null;// TODO, add  notification for me
                }
         //       List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(response);

                // Construct a message with just the exercises
                var exercisesMessage = response;

                return exercisesMessage;
            }
            catch (Exception e)
            {

                return $"נו..... {exercise}";
            }


        }

        private async Task<List<ExerciseModel>> ProcessAssistantResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
            {
                throw new Exception("The assistant's response is empty or null.");
            }

            // Validate that the response is a JSON array
            if (!response.TrimStart().StartsWith("["))
            {
                throw new Exception("שגיאה: התוכן שהתקבל אינו במבנה JSON המתאים.");
            }

            // Optionally, you can sanitize the response to remove any text before the JSON array
            int jsonStartIndex = response.IndexOf('[');
            if (jsonStartIndex > 0)
            {
                response = response.Substring(jsonStartIndex);
            }

            // Deserialize exercises
            try
            {
                List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(response);
                return exercises;
            }
            catch (JsonException ex)
            {
                // Log the error and response for debugging
                Console.WriteLine("JSON parsing error: " + ex.Message);
                Console.WriteLine("Response content: " + response);
                throw new Exception("שגיאה בעיבוד הנתונים שהתקבלו. אנא נסה שוב.");
            }
        }




    }
}