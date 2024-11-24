using BLL;
using BLL.Functions;
using BLL.Objects;
using BLL.Services;
using Mysqlx.Session;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio.TwiML.Messaging;


namespace BL.Serives
{
    public class IndexService
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly WhatsAppService _whatsAppService;
        private readonly ImgFunctions _imgFunctions;
        private readonly IHttpClientFactory _httpClientFactory;

        public IndexService(ExerciseRepository exerciseRepository, WhatsAppService whatsAppService, ImgFunctions imgFunctions, IHttpClientFactory httpClientFactory)
        {
            _exerciseRepository = exerciseRepository;
            _whatsAppService = whatsAppService;
            _imgFunctions = imgFunctions;
            _httpClientFactory = httpClientFactory;
        }
        public async Task<string> UserBalancer(string phoneNumber, string? body, List<string>? MediaUrl0, List<string>? MediaContentType0)
        {
            bool isValid = true;

            string result = string.Empty;

            var areAllSameType = MediaContentType0?.Distinct().Count() == 1;
            if ((areAllSameType) && MediaContentType0 != null && MediaContentType0[0] == "image/jpeg" && MediaUrl0 != null)
            {
                string? res = string.Empty;
                foreach (var item in MediaUrl0)
                {
                     res = await _imgFunctions.HandleImage(item);
                }
               
            }

            if (string.IsNullOrEmpty(body))
            {
                //isValid = false;
                //result = "אולי שלחתם משהו אבל לא רשמתם לי מה לעשות עם זה..??";
                body = "";
            }
            //         await _whatsAppService.SendExerciseToStudent(phoneNumber, body);
            var numericPhoneNumber = Regex.Replace(phoneNumber, "[^0-9]", "");
            var userType = await _exerciseRepository.GetUserRoleByPhoneNumber(numericPhoneNumber);

            if (isValid)
            {
                if (userType != null)
                {
                    
                    switch (userType.UserType)
                    {
                        case Constants.Teacher:
                           result = await HandleTeacherMessage(numericPhoneNumber, body);
                         //result = await HandleStudentMessage(numericPhoneNumber, body); //FOR TEST IF NEED AS STUDENT
                            break;
                        case Constants.Parent:
                            result = await HandleParentMessage(numericPhoneNumber, body);
                            break;
                        case Constants.Student:
                            result = await HandleStudentMessage(numericPhoneNumber, body);
                            break;
                        default:
                            result = "User type not recognized.";
                            break;
                    }
                }
                else
                {
                    result = "משתמש לא רשום, יש לפנות ל 0544345287";
                }
            }

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    //template: 972527332312@c.us 
                    await SendResponseToSender(phoneNumber, result);
                }
                catch (Exception e)
                {

                    Console.WriteLine(e.Message);
                }
                 //_whatsAppService.SendMessageToUser(phoneNumber, result);
            }

            return $"{result}";
        }



        private async Task<string> HandleStudentMessage(string phoneNumber, string studentMessage)
        {
            // Get the student ID based on the phone number
            int studentId = await _exerciseRepository.GetStudentIdByPhoneNumber(phoneNumber);
            var inProgressExercise = await _exerciseRepository.GetInProgressExercise(studentId);
            // Normalize the incoming message
            string normalizedMessage = studentMessage.Trim().ToLower();

            if (normalizedMessage == "test")
            {
                await SendImageToSender(phoneNumber, "10InRow_", "");
                var num = await _exerciseRepository.GetLastCorrectAnswers(13);
                await _whatsAppService.GetHelpForStudent("2 * 43");
                //await SendImageToSender(phoneNumber, "5_", "");

                return "";


               
            }
                // Check if the message is a command
                if (normalizedMessage == "start")
            {
                // Start the exercise sequence
                return await StartExercisesForStudent(studentId, phoneNumber);
            }
            else if (normalizedMessage == "help")
            {
                // Provide help for the current exercise
                return await ProvideHelpForStudent(studentId, phoneNumber);
            }
            else if (normalizedMessage == "דלג" && inProgressExercise.IncorrectAttempts > 3)
            {
                if (inProgressExercise != null)
                {
                    // Mark the current exercise as skipped by setting IsSkipped to true
                    await _exerciseRepository.SkipCurrentExercise(inProgressExercise.ProgressId);

                    // Fetch the next exercise for the student
                     return await StartExercisesForStudent(studentId, phoneNumber);
                }
                else
                {
                    return TextGeneratorFunctions.GetCcompletionMessages();
                }
            }
            if (normalizedMessage == "לא")
            {
                // Start the exercise sequence
                await SendImageToSender(phoneNumber, "getback_", "");
                return "";
            }

            else
            {
                var res = await ProcessStudentAnswer(studentId, phoneNumber, studentMessage);
                if (res=="0")
                {
                    return await StartExercisesForStudent(studentId, phoneNumber);
                }
                // Assume the message is an answer to an exercise
                return res;
            }
        }

        private async Task<string> StartExercisesForStudent(int studentId, string phoneNumber)
        {
            var inProgressExercise = await _exerciseRepository.GetInProgressExercise(studentId);
            if (inProgressExercise != null)
            {
                return $"You already have an exercise in progress:\n{inProgressExercise.Exercise}\nPlease answer it.";
            }
            //When say "כן"

            var nextExercise = await _exerciseRepository.GetNextUnassignedExercise(studentId);
            if (nextExercise.changeType != null)
            {
                string sendingText = TextGeneratorFunctions.GetLevelUpdateMessage(nextExercise.difficultyUpdate, nextExercise.changeType);
                string picType = nextExercise.changeType == "Upgraded" ? "levelupHard_" : "levelup_";
                await SendImageToSender(phoneNumber, picType, "");
                await SendResponseToSender(phoneNumber, sendingText);
            }

            if (nextExercise.exercise != null)
            {
                await _exerciseRepository.AddExerciseToStudentProgress(studentId, nextExercise.exercise.ExerciseId, false);
                
                return TextGeneratorFunctions.GetRandomExerciseMessage(MathFunctions.FormatExerciseString(nextExercise.exercise.Exercise));
            }
            else
            {
                await SendImageToSender(phoneNumber, "noExerciseLeft_", "");
                return TextGeneratorFunctions.GetFinishedExercisesMessage();

            }
        }


        private async Task<string> ProvideHelpForStudent(int studentId, string phoneNumber)
        {
            var inProgressExercise = await _exerciseRepository.GetInProgressExercise(studentId);
            if (inProgressExercise != null)
            {
                string helpContent = await _exerciseRepository.GetHelpForExercise(inProgressExercise.ExerciseId, studentId);
                if (string.IsNullOrEmpty(helpContent))
                {
                    helpContent = "No help is available for this exercise.";
                }


                string[] hintParts = Regex.Split(helpContent, @"(?<=[.!?])");

                string userFriendlyMessage = string.Join("\n", hintParts.Select(part => part.Trim()).Where(part => !string.IsNullOrWhiteSpace(part)));
                string formattedMessage = $"\u202B טעות קטנה ❌ 😏\n*{MathFunctions.FormatExerciseString(inProgressExercise.Exercise)}*\nטיפ:\n⬇\n*{userFriendlyMessage}*";

                return formattedMessage;

            }
            else
            {
                await SendImageToSender(phoneNumber, "noExerciseLeft_", "");
                return TextGeneratorFunctions.GetFinishedExercisesMessage();

            }
        }

        //        private async Task<string> ProcessStudentAnswer(int studentId, string phoneNumber, string studentAnswer)
        //        {
        //            // Remove non-digit characters
        //            studentAnswer = Regex.Replace(studentAnswer, @"\D", "");

        //            string exerciseText = string.Empty;

        //            // Check if the student has started any exercises
        //            if (!await _exerciseRepository.CheckIfStudentStarted(studentId))
        //            {
        //                var firstExercise = await _exerciseRepository.GetNextUnassignedExercise(studentId);
        //                if (firstExercise == null) return "0";

        //                await _exerciseRepository.AddExerciseToStudentProgress(studentId, firstExercise.ExerciseId, true);

        //                 exerciseText = (firstExercise.Exercise.Contains("?") || firstExercise.Exercise.Contains("_"))
        //                    ? firstExercise.Exercise
        //                    : $"? = {firstExercise.Exercise}";

        //                exerciseText = "‪" + exerciseText; // Add LTR character

        //                return $"תרגיל ראשון: 😍\n{exerciseText}";
        //            }

        //            var inProgressExercise = await _exerciseRepository.GetInProgressExercise(studentId);
        //            if (inProgressExercise == null) return "0";

        //            if ((DateTime.Now - inProgressExercise.UpdatedAt).TotalHours < 5 && inProgressExercise.IncorrectAttempts < 2)
        //            {
        //                exerciseText = MathFunctions.FormatExerciseString(inProgressExercise.Exercise);
        //                return $"👋 היי! שמתי לב שלא סיימת את התרגיל האחרון 😊. אל דאגה! ✨ הנה הוא שוב כדי לנסות שוב: ✏️💪\n\n{exerciseText}\n\n📚🚀";
        //            }

        //            int incorrectAttempts = inProgressExercise.IncorrectAttempts; //await _exerciseRepository.GetIncorrectAttempts(studentId, inProgressExercise.ExerciseId);

        //            if (incorrectAttempts >= 2)
        //            {
        //                await SendResponseToSender(phoneNumber, @"
        //כבר שולח עזרה..... ✨📝
        //👦📚👧
        //📖  בדרך...  📖
        //💡✨💬
        //");

        //                string chatGptResponse = await _whatsAppService.GetHelpForStudent(inProgressExercise.Exercise);
        //                await _exerciseRepository.UpdateGptHelpUsed(studentId, inProgressExercise.ExerciseId);

        //                exerciseText = MathFunctions.FormatExerciseString(inProgressExercise.Exercise);

        //                return $"תשובה לא נכונה 🙁. נראה שאתם מתקשים. הנה עזרה ממני 🆘:\n{chatGptResponse}\n\n⬇️ הנה התרגיל שלך ⬇️\n\n{exerciseText}\n";
        //            }

        //            string randomPhrase = TextGeneratorFunctions.GetMotivated();
        //            //exerciseText = (inProgressExercise.Exercise.Contains("_") || inProgressExercise.Exercise.Contains("\n"))
        //            //    ? inProgressExercise.Exercise
        //            //    : $"{inProgressExercise.Exercise} = ?";

        //            //exerciseText = "‪" + exerciseText;

        //            exerciseText = MathFunctions.FormatExerciseString(inProgressExercise.Exercise);

        //            return $"תשובה לא נכונה 🙁.\n{randomPhrase} 😎.\nתרגיל:\n {exerciseText}\n\nלעזרה, יש לכתוב help  🆘\n ";
        //        }

        #region studentAnswer --old
        private async Task<string> ProcessStudentAnswer(int studentId, string phoneNumber, string studentAnswer)
        {
            // Remove commas from the studentAnswer
            studentAnswer = studentAnswer.Replace(",", "");
            studentAnswer = Regex.Replace(studentAnswer, @"\D", "");
            var hasStarted = await _exerciseRepository.CheckIfStudentStarted(studentId);

            if (!hasStarted)
            {

                // This is the first time the student is sending a message
                var firstExercise = await _exerciseRepository.GetNextUnassignedExercise(studentId);
                if (firstExercise.changeType != null)
                {
                    string sendingText = TextGeneratorFunctions.GetLevelUpdateMessage(firstExercise.difficultyUpdate, firstExercise.changeType);
                    string picType = firstExercise.changeType == "Upgraded" ? "levelupHard_" : "levelup_";
                    await SendImageToSender(phoneNumber, picType, "");
                    await SendResponseToSender(phoneNumber, sendingText);
                }
                if (firstExercise.exercise != null)
                {
                    try
                    {
                        await _exerciseRepository.AddExerciseToStudentProgress(studentId, firstExercise.exercise.ExerciseId, true);
                    }
                    catch (Exception e)
                    {

                        Console.WriteLine($"error met 1: {e.Message}");
                    }

                    //      _ = _whatsAppService.SendExerciseToStudent(phoneNumber, firstExercise.Exercise);
                    string exerciseText = (firstExercise.exercise.Exercise.Contains("?") || firstExercise.exercise.Exercise.Contains("_"))
                        ? firstExercise.exercise.Exercise
                        : $"? = {firstExercise.exercise.Exercise}";

                    // Add the LTR character
                    exerciseText = "‪" + exerciseText;

                    return $"תרגיל ראשון: 😍\n{exerciseText}";

                }
                else
                {
                    return "0";
                }
            }

            //-------------------------------------
            var inProgressExercise = await _exerciseRepository.GetInProgressExercise(studentId);



            if (inProgressExercise != null)
            {
                // Check if the exercise was last updated less than 5 hours ago
                bool isCorrect = string.Equals(studentAnswer.Trim(), inProgressExercise.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
                //if left a while ago and answered wrong
                if ((DateTime.Now - inProgressExercise.UpdatedAt).TotalHours > 1 && !isCorrect)
                {
                    await _exerciseRepository.UpdateStudentProgressTimestamp(inProgressExercise.ProgressId);
                    string exerciseText = MathFunctions.FormatExerciseString(inProgressExercise.Exercise);
                    return $"👋 היי! שמתי לב שלא סיימת את התרגיל האחרון 😊. אל דאגה! ✨ הנה הוא שוב כדי לנסות שוב: ✏️💪\n\n{exerciseText}\n\n📚🚀";
                }

                // Update student progress in the database
                if (isCorrect)
                {
                    await _exerciseRepository.UpdateStudentProgress(studentId, inProgressExercise.ExerciseId, studentAnswer, isCorrect);
                    int exercisesSolvedToday = await _exerciseRepository.GetExercisesSolvedToday(studentId);

                    var lastCurrectAnswersInRow = await _exerciseRepository.GetLastCorrectAnswers(studentId);

                    if (exercisesSolvedToday % 5 == 0 && exercisesSolvedToday % 10 != 0 && (lastCurrectAnswersInRow > 0 &&  lastCurrectAnswersInRow % 5 != 0))
                    {
                        await SendImageToSender(phoneNumber, "5_", "");
                        //await SendResponseToSender(phoneNumber, TextGeneratorFunctions.Get5ExerciseSolvedMessage());
                        Thread.Sleep(1000);
                        // Send a congratulatory message to the student
                        string congratulatoryMessage = TextGeneratorFunctions.GetRandomCongratulatoryMessage(exercisesSolvedToday);

                        await SendResponseToSender(phoneNumber, congratulatoryMessage);
                    }

                    else if (exercisesSolvedToday % 10 == 0 && (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow % 10 != 0))
                    {
                        await SendImageToSender(phoneNumber, "final_", "");
                        string congratulatoryMessage = $"כל הכבוד על פתרון {exercisesSolvedToday} תרגילים היום! 💪✨ האם תרצה להמשיך?";


                        await SendResponseToSender(phoneNumber, congratulatoryMessage);
                        return "";
                    }

                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow % 5 == 0 && lastCurrectAnswersInRow % 10 != 0)
                    {
                        await SendImageToSender(phoneNumber, "5InRow_", "");
                        Thread.Sleep(1000);
                    }
                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow % 10 == 0)
                    {
                        await SendImageToSender(phoneNumber, "10InRow_", "");
                        Thread.Sleep(1000);
                    }

                    // Fetch the next unassigned exercise
                    var nextExercise = await _exerciseRepository.GetNextUnassignedExercise(studentId);
                    if (nextExercise.changeType != null)
                    {
                        string sendingText = TextGeneratorFunctions.GetLevelUpdateMessage(nextExercise.difficultyUpdate, nextExercise.changeType);
                        string picType = nextExercise.changeType == "Upgraded" ? "levelupHard_" : "levelup_";
                        await SendImageToSender(phoneNumber, picType, "");

                        await SendResponseToSender(phoneNumber, sendingText);
                    }
                    var exrcisesLeft = await _exerciseRepository.GetExercisesLeftForStudent(studentId);
                    if (nextExercise.exercise != null)
                    {
                        // Assign the exercise to the student
                        await _exerciseRepository.AddExerciseToStudentProgress(studentId, nextExercise.exercise.ExerciseId, false);

                        // Return message with next exercise
                        string exerciseText;

                        exerciseText = MathFunctions.FormatExerciseString(nextExercise.exercise.Exercise);

                        // Remove any asterisks if they were added accidentally or for formatting



                        string randomCorrectAnswer = TextGeneratorFunctions.GetCorrectAnswerText();

                        Random random2 = new Random();
                        bool includeAscii = random2.Next(0, 3) == 0; // 1 in 5 chance to include an ASCII drawing

                        string exercisesLeftText = exrcisesLeft < 10 ? $"🔥 נותרו לך עוד {exrcisesLeft} תרגילים לסיום. 💪✨\n" : "";

                        string response = $"{randomCorrectAnswer}\n{exercisesLeftText}\n{exerciseText}";


                        //if (includeAscii)
                        //{
                        //    response += "\n" + asciiFunction.randomDraw();
                        //}
                        //else
                        //{
                        //    response += "\n" + asciiFunction.emojisDraw();
                        //}

                        return response;

                    }
                    else
                    {
                        //
                        await SendImageToSender(phoneNumber, "noExerciseLeft_", "");
                        return TextGeneratorFunctions.GetFinishedExercisesMessage();
                    }
                }
                else
                {
                    await _exerciseRepository.UpdateStudentProgress(studentId, inProgressExercise.ExerciseId, studentAnswer, isCorrect);
                    int incorrectAttempts = inProgressExercise.IncorrectAttempts; //await _exerciseRepository.GetIncorrectAttempts(studentId, inProgressExercise.ExerciseId);
                    //incorrect attemp +1 that added to db
                    if (incorrectAttempts== 2)
                    {
                        await _exerciseRepository.SkipCurrentExercise(inProgressExercise.ProgressId);
                        string skipNext = "😊 *היי! קשה קצת? אין בעיה, דילגנו לתרגיל הבא ✨✏️. בהצלחה!* 🚀";
                        await SendResponseToSender(phoneNumber, skipNext);
                        return await StartExercisesForStudent(studentId, phoneNumber);
                    }

                    if (incorrectAttempts == 1)
                    {

                        string helpMessage = @"
        כבר שולח עזרה..... ✨📝
        👦📚👧
            📖  בדרך...  📖
              💡✨💬

        ";
                       // await SendResponseToSender(phoneNumber, helpMessage);
                       // await Task.Delay(1500);
                        await SendImageToSender(phoneNumber, "helpOnTheWay_", "");

                        string chatGptResponse = await _whatsAppService.GetHelpForStudent(inProgressExercise.Exercise);

                        await _exerciseRepository.UpdateGptHelpUsed(studentId, inProgressExercise.ExerciseId);

                        string skipText = string.Empty;
                        if (incorrectAttempts==3)
                        {
                             skipText = "*✨ אם ברצונך לדלג על התרגיל הזה, יש להקליד: דלג 💨*";
                        }

                        string exerciseFormatted =  MathFunctions.FormatExerciseString(inProgressExercise.Exercise).PadLeft(0, ' '); // Adjust the padding value as needed to align properly
                        string response = $"{skipText}\n\nתשובה לא נכונה 🙁. נראה שאתם מתקשים. הנה עזרה ממני 🆘:\n{chatGptResponse}\n\n⬇️ הנה התרגיל שלך ⬇️\n\n{exerciseFormatted}\n";
                        return response;
                    }

                    //################
                    //# Second chance#
                    //################
                    string exerciseText;
                    exerciseText = MathFunctions.FormatExerciseString(inProgressExercise.Exercise);

                    

                    var randomPhrase = TextGeneratorFunctions.GetMotivated();
                    //at the first rount, give hint (its 0 on the first inccorect answer)
                    if (incorrectAttempts ==0) return await ProvideHelpForStudent(studentId, phoneNumber);

                    return $"תשובה *לא* נכונה 🙁.\n{randomPhrase} 😎.\nתרגיל:\n {exerciseText}\n\nלעזרה, יש לכתוב help  🆘\n ";




                }
            }
            else
            {
                // No in-progress exercise
                return "0";
            }
        }

        #endregion

        //********************
        // Services/IndexService.cs
        private async Task<string> HandleTeacherMessage(string phoneNumber, string teacherMessage)
        {
            (int teacherId, int classId) = await _exerciseRepository.GetTeacherIdByPhoneNumber(phoneNumber);

            string normalizedMessage = teacherMessage.Trim().ToLower();

            if (normalizedMessage == "lead")
            {
                var leaderboard = await _exerciseRepository.GetLeaderboardForClassByTeacherId(teacherId);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*Class Leaderboard:*");
                sb.AppendLine("```");
                sb.AppendLine("Rank  | Name           | Points");
                sb.AppendLine("-----------------------------");
                int rank = 1;
                foreach (var student in leaderboard)
                {
                    sb.AppendLine($"{rank,-5}| {student.FullName,-15}| {student.CorrectAnswers,6}");
                    rank++;
                }
                sb.AppendLine("```");

                return sb.ToString();
            }

            else if (normalizedMessage == "status")
            {
                var classProgress = await _exerciseRepository.GetClassProgress(teacherId);
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("*📊 סטטוס כיתתי:*"); // Hebrew: Class Progress
                sb.AppendLine("```");
                sb.AppendLine("🧑‍🎓 שם התלמיד      | ✅ תשובות נכונות"); // Hebrew: Student Name | Correct Answers
                sb.AppendLine("-------------------------------");
                foreach (var student in classProgress.Students)
                {
                    sb.AppendLine($"{student.FullName,-15}| {student.CorrectAnswers,7}");
                }
                sb.AppendLine("```");

                return sb.ToString();
            }


            else if (normalizedMessage.Contains("***"))
            {
                string instructionText = string.Empty;
                string exampleText = string.Empty;
                string difficultyLevel = string.Empty;

                var messageParts = normalizedMessage.Split("***", StringSplitOptions.RemoveEmptyEntries);
                if (messageParts.Length > 0)
                {
                    var instructionAndExample = messageParts[0].Split("//", StringSplitOptions.RemoveEmptyEntries);
                    instructionText = instructionAndExample.ElementAtOrDefault(0)?.Trim() ?? string.Empty;
                    exampleText = instructionAndExample.ElementAtOrDefault(1)?.Trim() ?? string.Empty;

                    // Check if instructionText contains "###"
                    if (instructionText.Contains("###"))
                    {
                        var splitInstruction = instructionText.Split("###", StringSplitOptions.RemoveEmptyEntries);
                        instructionText = splitInstruction.ElementAtOrDefault(0)?.Trim() ?? string.Empty;
                        difficultyLevel = splitInstruction.ElementAtOrDefault(1)?.Trim() ?? string.Empty;
                    }

                    // Check if exampleText contains "###"
                    if (exampleText.Contains("###"))
                    {
                        var splitExample = exampleText.Split("###", StringSplitOptions.RemoveEmptyEntries);
                        exampleText = splitExample.ElementAtOrDefault(0)?.Trim() ?? string.Empty;
                        difficultyLevel = splitExample.ElementAtOrDefault(1)?.Trim() ?? string.Empty;
                    }
                    else if (string.IsNullOrEmpty(difficultyLevel) && messageParts.Length > 1) // Handle difficultyLevel from messageParts[1]
                    {
                        difficultyLevel = messageParts[1].Trim();
                    }
                }

                string midMessage = "\u200F מחולל 💡, יש למתין...⌛✨";

                await SendResponseToSender(phoneNumber, midMessage);
                //              await _whatsAppService.SendMessageToUser(phoneNumber, midMessage);
                difficultyLevel = "Easy";
                return await _whatsAppService.GetExercisesFromGPT(exampleText, teacherId, Constants.Teacher, classId, instructionText, difficultyLevel); //TODO: change the 1, now its for test cos same user is teacher and student
            }

            //remind students
            else if (normalizedMessage.Contains("reminder"))
            {

                normalizedMessage = normalizedMessage.Normalize(NormalizationForm.FormC);
                string[] parts = normalizedMessage.Split(',');
                if (parts.Length != 3) return "";
               
                    var classText = parts[1].Trim();
                    string textToSend = parts[2].Trim();

                    var studentsList = await _exerciseRepository.GetUsersByClassAsync(classText);

                foreach (var item in studentsList)
                {
                  //  textToSend = $"{item.FullName}\n איפה נעלמת?? \n {textToSend}";
                     textToSend = @$"
                          ♪
                           ♫♫
                            ♫♫♫
                             ♫♫♫♫
                            ♫♫♫
                           ♫♫
                           ♪
                :.ılı.——{item.FullName} לא שמענו ממך מזמן——.ılı.
                                         
                :    ▄ █ ▄ █ ▄ ▄ █ ▄ █ ▄ █
                : Min- – – – – – – – – -●Max
                
                📣 מה דעתכם לפתור תרגיל ביחד?
                    ✏️ בואו נתקדם! 🚀😄💪

        ";
                    await SendResponseToSender(item.PhoneNumber, textToSend);
                }
                return string.Empty;
            }
            else
            {
                string exerciseText = "\u202A" + "*** 12 + _ = 14 ";
                string exerciseText2 = "\u202A" + "*** 12 * 6 =  ";
                return "\u200F *פקודה לא מוכרת.* \n" +
                       "\u200Fהנה הפקודות הזמינות שתוכל לשלוח:\n" +
                       "1. *lead*: \n" +
                       "\u200Fקבל את לוח התוצאות של הכיתה.\n" +
                       "2. *status*: \n" +
                       "\u200Fקבל את סטטוס ההתקדמות של כל תלמיד.\n" +
                       "3. *<הודעה עם ***>*: \n" +
                       "\u200F צור 10 תרגילים חדשים. דוגמאות:\n" +
                       $"{exerciseText}\n" +
                       $"{exerciseText2}";
            }


        }

        private async Task<string> HandleParentMessage(string phoneNumber, string parentMessage)
        {
            int parentId = await _exerciseRepository.GetParentIdByPhoneNumber(phoneNumber);

            // Logic for handling parent messages
            // For example, parents can create exercises for their kids

            return "Parent functionality is under development.";
        }

        private async Task SendResponseToSender(string number, string message)
        {
            // Prepare the data to send to Node.js server
            var client = _httpClientFactory.CreateClient();
            var requestUri = "http://localhost:3000/send-message"; // Assuming Node.js server runs locally on port 3000

            var payload = new
            {
                number = number.Replace("@c.us", ""), // Strip the "@c.us" part if needed
                message = message
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Send the HTTP request to the Node.js server
            var response = await client.PostAsync(requestUri, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Message successfully sent to {number}");
            }
            else
            {
                Console.WriteLine($"Failed to send message to {number}: {response.StatusCode}");
            }
        }

        private async Task SendImageToSender(string number, string imageFileName, string type)
        {
            // Prepare the data to send to Node.js server
            var client = _httpClientFactory.CreateClient();
            var requestUri = "http://localhost:3000/send-image"; // Assuming Node.js server runs locally on port 3000

            var payload = new
            {
                number = number.Replace("@c.us", "").Trim().Replace("+", ""), // Normalize the number
                imageFileName = imageFileName // Match the Node.js expected property
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            // Send the HTTP request to the Node.js server
            var response = await client.PostAsync(requestUri, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Image successfully sent to {number}");
            }
            else
            {
                Console.WriteLine($"Failed to send image to {number}: {response.StatusCode}");
            }
        }





    }
}
