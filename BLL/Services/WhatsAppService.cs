using BL.Serives;
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

            string gptQuery = "Please create 20 unique math exercises:" + instructions + " using the **same mathematical operation and format** as the given example. difficulti level"+ level + " Example: " + example + ".\n\n" +
"**Requirements:**\n" +
"- Each generated exercise **must use the same operation** (e.g., multiplication, division, addition, subtraction) as the given example and must have the same level of complexity.\n" +
"- Ensure each exercise has **only two operands**, if that is the case for the example. Maintain similar operand sizes to ensure the difficulty level is consistent.\n" +
"- Use **random numbers** for operands. The exercises must not use sequential patterns like 1 * 1, 1 * 2, 1 * 3, etc.\n" +
"- Randomize the numbers in a way that no clear pattern (e.g., incremental, decremental) can be observed across the exercises.\n" +
"- Limit the numbers to reasonable ranges if necessary but ensure diversity and randomness.\n\n" +
"- Provide the correct answer as a numeric value without additional text (e.g., no 'x = ...').\n\n" +

"**Hints Requirements:**\n" +
"- For each exercise, provide a hint that explains how to solve the problem in a step-by-step manner suitable for an 8-10 year old child.\n" +
"- Hints should be in simple Hebrew and should include common mistakes that children might make, explained in a friendly way.\n" +
"- Add playful encouragement and relatable errors to keep young students engaged.\n" +
"- **Do not provide the answer in the hint**. Focus only on explaining the method.\n" +
"- Make the hint short and clear (2-3 sentences maximum).\n\n" +


"**Example Hint:**\n" +
"- For instance, if the exercise is *12 × 6*:\n" +
"' 🎉. תזכרו שאם קשה לחשב ישר, אפשר לחלק את ה-12 ל-10 ו-2, ואז לכפול כל חלק ב-6 ולהוסיף אותם בסוף 😊.כפלו תחילה 6 ב 2: 6*2 = 12 ואז 6 * 10 = 60.  אז התשובה תהיה: 12  + 60.'\n\n" +


"**Output Requirements:**\n" +
"- Your response **must be in a valid JSON array format**.\n" +
"- [{ \"\"exercise\"\": \"\"A + B\"\", \"\"answer\"\": C, \"\"hint\"\": \"\"Your concise hint here.\"\", \"\"DifficultyLevel\"\": \"\"Easy\"\" }].\n" +
" - Ensure each object has a `DifficultyLevel` field with values: \"Easy\", \"Medium\", or \"Hard\"." +
"- **Return only the JSON array**, with no additional text or characters.\n\n" +


"**Remember**: Use lots of emojis to make the hints fun and engaging for kids 🎈🚀🖍️📚✨😊.  do not add any special characters, do not add json or anything else before or after the JSON array. Return **only** the JSON array, and nothing else.\n\n";



            try
            {
                example = example.StartsWith("***") ? example.Substring(3).Trim() : example;

                var response = await _chatGPTService.AskChatGPT(gptQuery);
                var res = await _exerciseRepository.SaveExercisesToDatabase(response, creatorUserId, creatorRole, classId);
                if (response == null)
                {
                    return null;// TODO, add  notification for me
                }
                List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(response);

                // Construct a message with just the exercises
                var exercisesMessage = "התרגילים שנוצרו :\n" + string.Join("\n", exercises.Select(e => e.Exercise));

                return exercisesMessage;
            }
            catch (Exception e)
            {

                return $"error {e.Message}";
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

        

        

    }
}
