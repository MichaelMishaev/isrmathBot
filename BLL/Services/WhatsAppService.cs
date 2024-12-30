using BL.Serives;
using BLL.Functions;
using BLL.Objects;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class WhatsAppService
    {
        private readonly ChatGPTService _chatGPTService;
        private readonly ExerciseRepository _exerciseRepository;
        private readonly IConfiguration _configuration;
        private readonly CommonFunctions _commonFunctions;
        // Inject ChatGPTService through the constructor
        public WhatsAppService(ChatGPTService chatGPTService, ExerciseRepository exerciseRepository, IConfiguration configuration, CommonFunctions commonFunctions)
        {
            _chatGPTService = chatGPTService;
            _exerciseRepository = exerciseRepository;
            _configuration = configuration;
            _commonFunctions = commonFunctions;
        }

        public async Task<string> GetExercisesFromGPT(string example, int teacherId, string creatorRole, int classId, string instructions,int? creatorUserId,bool isMultipleChoise, string classInstruction = "")
        {

            string gptQuery = "Please create 25 unique math exercises " + instructions + " using the **same mathematical operation and format** as the given example. Example: " + example + ".\n\n" +

"**Additional Requirements for Comparison Questions:**\n" +
//"- If the exercise involves determining which side is greater, smaller, or equal, you must provide a comparison in a way that the final answer is a numeric choice from 1 to 3.\n" +
//"- Do not use symbols like '<', '>', or '=' in the 'answer' field. Instead, follow these rules:\n" +
//"  1: if the left side is larger than the right side\n" +
//"  2: if the left side is smaller than the right side\n" +
//"  3: if both sides are equal\n" +
//"- **Do not perform any comparisons  unless explicitly asked to compare.**\n" +
//" **provide a numeric answer** unless asked anoter answer to be provided" +
//"- Always perform the calculation first and then determine which of the three numeric answers (1, 2, or 3) is correct, but only if requested.\n" +
//"- The exercise should present a question in Hebrew like: 'מה יותר גדול:' followed by the two sides, and instructions to choose 1, 2, or 3 according to the above rules.\n" +
//"- Ensure that the final 'answer' field in the JSON is just '1', '2', or '3', with no additional symbols.\n" +
//"- **When asked to compare numbers, use the logic specified above for determining the correct numeric answer (1, 2, or 3).**\n\n" +

"**Requirements:**\n" +
"- **Accuracy is critical**: Incorrect answers are not acceptable. Ensure all answers are correct and consistent with the calculations.\n" +
"- Each generated exercise **must use the same operation** (e.g., multiplication, division, addition, subtraction) as the given example and must have the same level of complexity. Recheck the answers you send.\n" +
"- Ensure each exercise has **only two operands**, if that is the case for the example. Maintain similar operand sizes to ensure the difficulty level is consistent. Recheck the answers you send.\n" +
"- **Ensure that the exercises are different from each other and from the example provided.**\n" +
"- Use **random numbers** for operands. The exercises must not use sequential patterns like 1 × 1, 1 × 2, 1 × 3, etc.\n" +
"- Randomize the numbers in a way that no clear pattern (e.g., incremental, decremental) can be observed across the exercises.\n" +
"- Limit the numbers to reasonable ranges if necessary but ensure diversity and randomness.\n" +
"- Very important to follow the example: " + example + ".\n" +
"- Provide the correct answer as a numeric value (1, 2, or 3 etc) without additional text **only when explicitly asked to compare**.\n" +
"- Recheck the answers you send!\n\n" +
"- For each exercise, provide an internal explanation of how you arrived at the comparison result. This explanation will not be included in the final JSON but must be part of your internal process to ensure accuracy.\n\n" +

"**Hints Requirements:**\n" +
"- For each exercise, provide a hint that explains how to solve the problem in a step-by-step manner suitable for an 8-10-year-old child.\n" +
"- Hints should be in simple Hebrew and should include common mistakes that children might make, explained in a friendly way.\n" +
"- Add playful encouragement and relatable errors to keep young students engaged.\n" +
"- **Do not provide the answer in the hint.** Focus only on explaining the method.\n" +
"- Make the hint short and clear (2-3 sentences maximum).\n" +
"- **Avoid using double quotation marks (\\\") inside the hints to prevent JSON formatting issues. Use single quotes (') if necessary.**\n\n" +

"- **Return a valid JSON array** with double quotes for field names and string values. Do not escape double quotes in the JSON response.\n" +
"- Ensure the response can be parsed directly as a JSON array in C#.\n\n" +

"**Example Hint:**\n" +
"- For instance, if the exercise is *12 × 6*:\n" +
"'🎉 תזכרו שאם קשה לחשב ישר, אפשר לחלק את ה-12 ל-10 ו-2, ואז לכפול כל חלק ב-6 ולהוסיף אותם בסוף 😊 כפלו תחילה 6 ב-2 ואז 6 ב-10, ואז חברו את התוצאות.'\n\n" +

"**Output Requirements:**\n" +
"- Your response **must be in a valid JSON array format**.\n" +
"- Example format:\n" +
"[\n" +
"  { \\\"exercise\\\": \\\"A × B\\\", \\\"answer\\\": \\\"C\\\", \\\"hint\\\": \\\"Your concise hint here.\\\", \\\"DifficultyLevel\\\": \\\"Easy\\\" },\n" +
"  ...\n" +
"]\n" +
"- Ensure each object has a DifficultyLevel field with values: \\\"Easy\\\", \\\"Medium\\\", or \\\"Hard\\\".\n" +
"- **Return only the JSON array**, with no additional text or characters before or after it.\n" +
"- **Ensure all strings are properly escaped according to JSON standards, especially in the hints. Do not include unescaped special characters or quotation marks within the strings. Recheck the answers you send.**\n\n" +

"**Remember**:\n" +
"- Use lots of emojis to make the hints fun and engaging for kids 🎈🚀🖍️📚✨😊.\n" +
"- Do not add any special characters or text before or after the JSON array.\n" +
"- Return **only** the JSON array, and nothing else.";








            try
            {

                bool isMultipleChoice = instructions.ToLower().Contains("multiple choice"); //TODO pass the param


                int? grade = null;
                classInstruction = classInstruction.Replace(" ", "").Replace("\u00A0", "");

                var gradeMatch = Regex.Match(classInstruction, @"GRADE=(\d+)", RegexOptions.IgnoreCase);
                var isUseInstructionMatch = Regex.Match(classInstruction, @"instruc=(\d+)", RegexOptions.IgnoreCase);

                //##################################
                int instructionId = -1;
                if (isUseInstructionMatch.Success)//instructions for explenation of exercise
                {
                    instructionId = int.Parse(isUseInstructionMatch.Groups[1].Value);
                }
                // Fetch the instruction from the database if instructionId is valid
                string instructionText = await _exerciseRepository.GetInstructionFromDatabase(instructionId);

                // Append the instruction to the main instructions if available
                if (!string.IsNullOrEmpty(instructionText))
                {
                    instructions += $"\n\n{instructionText}";
                }
                //##################################

                if (gradeMatch.Success)
                {
                    grade = int.Parse(gradeMatch.Groups[1].Value);
                }

                Stopwatch stopwatch = Stopwatch.StartNew();

                var response = await _chatGPTService.AskChatGPT(gptQuery);//, cancellationToken);

                stopwatch.Stop();
                double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

                if (string.IsNullOrWhiteSpace(response)) //|| string.IsNullOrWhiteSpace(response2))
                {
                    await _commonFunctions.SendResponseToSender("972544345287", $"{response}");
                    return "שגיאה ביצירת תרגילים. אנא נסה שוב.";
                }

                // Process and deserialize each response
                List<ExerciseModel> exercises1 = await ProcessAssistantResponse(response);

                if (instructionId  == 2)
                {
                    foreach (var ex in exercises1)
                    {
                        ex.InstructionId = 2;
                    }
                }
                // isMultiple choise add if
                foreach (var ex in exercises1)
                {
                    ex.QuestionType = isMultipleChoice ? "MultipleChoice" : "OpenAnswer";
                }


                string combinedResponse = JsonConvert.SerializeObject(exercises1);

                // Create a unique identifier for this pending exercise set
                string pendingId = Guid.NewGuid().ToString();

                int saveClassId = classId;
                if (grade.HasValue)
                {
                    // If targeting a grade, set ClassId = 0 to indicate grade-level assignment
                    saveClassId = 0;
                }

                // Save to temporary storage, deleting any existing pending exercises first
                await _exerciseRepository.SavePendingExercises(pendingId, combinedResponse, creatorUserId ?? 1, creatorRole, saveClassId, grade);


                // Construct a message with the exercises and ask for confirmation
                var exercisesMessage = $"🕒 זמן שנדרש: {elapsedSeconds:F2} שניות\n" +
                           $"📝 נוצרו {exercises1.Count} תרגילים\n\n" +
                           "התרגילים שנוצרו:\n" +
                           string.Join("\n", exercises1.Select(e => $"{e.Exercise} - תשובה נכונה: {e.CorrectAnswer}")) +
                           "\n\nהאם התרגילים מתאימים? כתוב 'כן' כדי לאשר ולשמור, או 'לא' כדי לספק הוראות חדשות.";




                return exercisesMessage;
            }
            catch (Exception e)
            {
                // Log the exception if necessary
                await _commonFunctions.SendResponseToSender("972544345287", $"exception on create exercises: {e.Message}");
                Console.WriteLine($"Error: {e.Message}");
                return $"שגיאה: {e.Message}";
            }
        }

        public async Task<string> GetHelpForStudent(string exercise, CancellationToken cancellationToken)
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

                var response = await _chatGPTService.AskChatGPTReduceTokens(gptQuery, cancellationToken);
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

                return $"אין טיפ זמין: {exercise}";
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
