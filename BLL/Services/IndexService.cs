using BLL;
using BLL.Functions;
using BLL.Objects;
using BLL.Services;
using Microsoft.IdentityModel.Tokens;
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
using Tesseract;



namespace BL.Serives
{
    public class IndexService
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly WhatsAppService _whatsAppService;
        private readonly ImgFunctions _imgFunctions;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly QuizService _quizService;


        public IndexService(ExerciseRepository exerciseRepository, WhatsAppService whatsAppService, ImgFunctions imgFunctions, IHttpClientFactory httpClientFactory, QuizService quizService)
        {
            _exerciseRepository = exerciseRepository;
            _whatsAppService = whatsAppService;
            _imgFunctions = imgFunctions;
            _httpClientFactory = httpClientFactory;
            _quizService = quizService;
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
            var userId = userType?.RoleSpecificId;


            //For teacher use, wait for theacher to approve or delete exercises created by api



            if (isValid)
            {
                if (userType != null)
                {

                    switch (userType.UserType)
                    {
                        case Constants.Teacher:
                            result = await HandleTeacherMessage(numericPhoneNumber, body, userId);
                          //  result = await HandleStudentMessage(numericPhoneNumber, body); //FOR TEST IF NEED AS STUDENT
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
            var activeQuiz = await _exerciseRepository.GetActiveQuizSession(studentId);
            // Normalize the incoming message
            string normalizedMessage = studentMessage.Trim().ToLower();

            if (normalizedMessage == "test")
            {
                string res;
                try
                {


                    //res = await  _quizService.StartQuiz(studentId, phoneNumber);
                }
                catch (Exception e)
                {

                    throw;
                }
                //await SendImageToSender(phoneNumber, "10InRow_", "");

                //await SendImageToSender(phoneNumber, "5InRow_", "");
                //await _whatsAppService.GetHelpForStudent("2 * 43");
                //await SendImageToSender(phoneNumber, "5_", "");

                return "";
            }

            if (activeQuiz != null)
            {
                return await _quizService.HandleQuizAnswer(studentId, (int)activeQuiz, phoneNumber, studentMessage);
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
            else if (normalizedMessage == "דלג" )
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
            if (normalizedMessage == "לא" || normalizedMessage == "ביי")
            {
                // Start the exercise sequence
                await SendImageToSender(phoneNumber, "getback_", "");
                return "";
            }

            else
            {
                var res = await ProcessStudentAnswer(studentId, phoneNumber, studentMessage);
                if (res == "0")
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




        private async Task<string> ProcessStudentAnswer(int studentId, string phoneNumber, string studentAnswer)
        {
            // Remove commas from the studentAnswer
            studentAnswer = studentAnswer.Replace(",", "");
            studentAnswer = Regex.Replace(studentAnswer, @"\D", "");
            var hasStarted = await _exerciseRepository.CheckIfStudentStarted(studentId);
            string studentName = await _exerciseRepository.GetUserFullNameByStudentIdAsync(studentId);

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
                if (inProgressExercise.IsWaitingForHelp)
                {
                    // Inform the student to wait and prevent further processing
                    await SendResponseToSender(phoneNumber, "📚 העזרה בדרך! אנא המתן לפני שתנסה שוב. 🕐");
                    return "";
                }
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

                    //########################################
                    //##############DB LOGICS#################
                    //########################################
                    await _exerciseRepository.UpdateStudentProgress(studentId, inProgressExercise.ExerciseId, studentAnswer, isCorrect);
                    int exercisesSolvedToday = await _exerciseRepository.GetExercisesSolvedToday(studentId);
                    await _exerciseRepository.UpdateIsWaitingForHelp(studentId, inProgressExercise.ExerciseId, false);//to prevent send message while waiting gpt answer
                    bool isFastAnswers = await _exerciseRepository.isStudentAnswerFast(studentId);//is to send congrat for fast answers
                    var lastCurrectAnswersInRow = await _exerciseRepository.GetLastCorrectAnswers(studentId);
                    var exrcisesLeft = await _exerciseRepository.GetExercisesLeftForStudent(studentId);

                    //#########################################
                    if (isFastAnswers)
                    {
                        await SendImageToSender(phoneNumber, "fastSolve_", "");
                        await SendResponseToSender(phoneNumber, "🔥 וואו! 10 תרגילים בדקה! לא יאומן! 🏆🎈");
                        Thread.Sleep(1000);
                    }

                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow == 5)
                    {

                        await SendImageToSender(phoneNumber, "5InRow_", "");
                        Thread.Sleep(1000);
                    }
                    else if (exercisesSolvedToday % 5 == 0 && exercisesSolvedToday % 10 != 0 && (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow % 5 != 0))
                    {
                        //await SendImageToSender(phoneNumber, "5_", "");
                        //await SendResponseToSender(phoneNumber, TextGeneratorFunctions.Get5ExerciseSolvedMessage());
                        //Thread.Sleep(1000);
                        // Send a congratulatory message to the student
                        string congratulatoryMessage = TextGeneratorFunctions.Get5ExerciseSolvedMessage(studentName);

                        await SendResponseToSender(phoneNumber, congratulatoryMessage);
                    }

                    else if (exercisesSolvedToday % 10 == 0 && (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow % 10 != 0))
                    {
                        if (exercisesSolvedToday == 10 && exrcisesLeft > 10)
                        {

                            await SendImageToSender(phoneNumber, "quiz_", "");
                            Thread.Sleep(2000);
                            var res = await _quizService.StartQuiz(studentId, phoneNumber);
                            return res;
                        }

                        await SendImageToSender(phoneNumber, "final_", "");
                        string congratulatoryMessage = $"כל הכבוד על פתרון {exercisesSolvedToday} תרגילים היום! 💪✨ בואו נמשיך?";


                        await SendResponseToSender(phoneNumber, congratulatoryMessage);


                        return "";
                    }


                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow == 10)
                    {
                        if (exercisesSolvedToday == 10 && exrcisesLeft > 10)
                        {

                            await SendImageToSender(phoneNumber, "quiz_", "");
                            Thread.Sleep(2000);
                            var res = await _quizService.StartQuiz(studentId, phoneNumber);
                            return res;
                        }

                        await SendImageToSender(phoneNumber, "10InRow_", "");
                        Thread.Sleep(1000);
                    }

                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow == 15)
                    {
                        await SendImageToSender(phoneNumber, "15InRow_", "");
                        Thread.Sleep(1000);
                    }

                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow == 20)
                    {
                        await SendImageToSender(phoneNumber, "20InRow_", "");
                        Thread.Sleep(1000);
                    }
                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow == 25)
                    {
                        await SendImageToSender(phoneNumber, "25InRow_", "");
                        Thread.Sleep(1000);
                    }
                    else if (lastCurrectAnswersInRow > 0 && lastCurrectAnswersInRow == 30)
                    {
                        await SendImageToSender(phoneNumber, "30InRow_", "");
                        Thread.Sleep(1000);
                    }

                    //topInRow_1.jpeg


                    // Fetch the next unassigned exercise
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
                        string skipText = TextGeneratorFunctions.GetSkipPromptMessage();
                        string response = $"{randomCorrectAnswer}\n{exercisesLeftText}\n{exerciseText}\n\n{skipText}";



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
                    if (incorrectAttempts == 2)
                    {
                        await _exerciseRepository.SkipCurrentExercise(inProgressExercise.ProgressId);
                        string skipNext = "😊 *היי! קשה קצת? אין בעיה, דילגנו לתרגיל הבא ✨✏️. בהצלחה!* 🚀";
                        await SendResponseToSender(phoneNumber, skipNext);
                        return await StartExercisesForStudent(studentId, phoneNumber);
                    }

                    if (incorrectAttempts == 1)
                    {

                        await _exerciseRepository.UpdateIsWaitingForHelp(studentId, inProgressExercise.ExerciseId, true);


                        await SendImageToSender(phoneNumber, "helpOnTheWay_", "");
                        //Thread.Sleep(10000);
                        //##################
                        _ = GenerateAndSendHelpAsync(studentId, phoneNumber, inProgressExercise);

                        //##################
                        //  string chatGptResponse = await _whatsAppService.GetHelpForStudent(inProgressExercise.Exercise);

                        //await _exerciseRepository.UpdateGptHelpUsed(studentId, inProgressExercise.ExerciseId);

                        //string skipText = string.Empty;
                        //if (incorrectAttempts == 3)
                        //{
                        //    skipText = "*✨ אם ברצונך לדלג על התרגיל הזה, יש להקליד: דלג 💨*";
                        //}

                        //string exerciseFormatted = MathFunctions.FormatExerciseString(inProgressExercise.Exercise).PadLeft(0, ' '); // Adjust the padding value as needed to align properly
                        //string response = $"{skipText}\n\nתשובה לא נכונה 🙁. נראה שאתם מתקשים. הנה עזרה ממני 🆘:\n{chatGptResponse}\n\n⬇️ הנה התרגיל שלך ⬇️\n\n{exerciseFormatted}\n";
                        return "";//response;
                    }

                    //################
                    //# Second chance#
                    //################
                    string exerciseText;
                    exerciseText = MathFunctions.FormatExerciseString(inProgressExercise.Exercise);



                    var randomPhrase = TextGeneratorFunctions.GetMotivated();
                    //at the first rount, give hint (its 0 on the first inccorect answer)
                    if (incorrectAttempts == 0) return await ProvideHelpForStudent(studentId, phoneNumber);

                    return $"תשובה *לא* נכונה 🙁.\n{randomPhrase} 😎.\nתרגיל:\n {exerciseText}\n ";




                }
            }
            else
            {
                // No in-progress exercise
                return "0";
            }
        }



        private async Task GenerateAndSendHelpAsync(int studentId, string phoneNumber, ExerciseModel inProgressExercise)
        {
            try
            {
                /////######### Create a CancellationTokenSource with a 20-second timeout
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)))
                {
                    /////######### Get help from GPT API with cancellation token
                    string chatGptResponse = await _whatsAppService.GetHelpForStudent(inProgressExercise.Exercise, cts.Token);

                    // Check if the student is still on the same exercise
                    var currentExercise = await _exerciseRepository.GetInProgressExercise(studentId);
                    if (currentExercise != null && currentExercise.ExerciseId == inProgressExercise.ExerciseId)
                    {
                        /////######### Reset the waiting state
                        await _exerciseRepository.UpdateIsWaitingForHelp(studentId, inProgressExercise.ExerciseId, false);

                        await _exerciseRepository.UpdateGptHelpUsed(studentId, inProgressExercise.ExerciseId);

                        string exerciseFormatted = MathFunctions.FormatExerciseString(inProgressExercise.Exercise).PadLeft(0, ' ');
                        string response = $"תשובה לא נכונה 🙁. הנה עזרה ממני 🆘:\n{chatGptResponse}\n\n⬇️ הנה התרגיל שלך ⬇️\n\n{exerciseFormatted}\n";

                        await SendResponseToSender(phoneNumber, response);
                    }
                    else
                    {
                        // The student has moved on; reset the waiting state
                        await _exerciseRepository.UpdateIsWaitingForHelp(studentId, inProgressExercise.ExerciseId, false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Timeout occurred
                /////######### Reset the waiting state
                await _exerciseRepository.UpdateIsWaitingForHelp(studentId, inProgressExercise.ExerciseId, false);

                // Inform the student that help is unavailable
                await SendResponseToSender(phoneNumber, "⏰ מצטערים, כרגע לא ניתן לספק עזרה. אנא נסה שוב או המשך לתרגיל הבא.");
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                /////######### Reset the waiting state
                await _exerciseRepository.UpdateIsWaitingForHelp(studentId, inProgressExercise.ExerciseId, false);

                Console.WriteLine($"Error generating help: {ex.Message}");

                // Optionally inform the student about the error
                await SendResponseToSender(phoneNumber, "אירעה שגיאה בעת יצירת העזרה. אנא נסה שוב מאוחר יותר.");
            }
        }



        //********************
        // Services/IndexService.cs
        private async Task<string> HandleTeacherMessage(string phoneNumber, string teacherMessage, int? userId)
        {
            var resTecherUpdate = await TecherExercisesCreationHandle(userId, teacherMessage);

            if (!string.IsNullOrEmpty(resTecherUpdate))
            {
                return resTecherUpdate;
            }
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
                sb.AppendLine("*📊 סטטוס כיתתי ב-7 ימים אחרונים:*"); // Hebrew: Class Progress
                sb.AppendLine("```");

                // Header line
                sb.AppendLine("שם התלמיד    | ✅  | ❌  | ⏭️ דילוגים | ממוצע (%)"); // Hebrew: Student | Correct | Wrong | Skips | Average (%)

                // Separator
                sb.AppendLine("---------------------------------------------");

                // Data rows
                foreach (var student in classProgress.Students)
                {
                    // Calculate average percentage
                    double totalAttempts = student.CorrectAnswers + student.WrongAnswers;
                    string average = totalAttempts > 0
                        ? $"{(student.CorrectAnswers / totalAttempts * 100):F2}" // Format as percentage
                        : "N/A"; // Handle cases with no attempts

                    // Append data row with proper alignment
                    sb.AppendLine(
                        $"{student.FullName,-15} | {student.CorrectAnswers,3} | {student.WrongAnswers,4} | {student.Skip,4} | {average,6}"
                    );
                }

                sb.AppendLine("```");

                return sb.ToString();
            }







            else if (normalizedMessage.StartsWith("#update"))
            {
                // Parse the normalizedMessage to extract the className
                var parts = normalizedMessage.Split(",", StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2)
                {
                    await SendResponseToSender(phoneNumber, "❌ Format incorrect. Use: #update, [ClassName]");
                    return "";
                }

                string className = parts[1].Trim();

                // Call the DAL function to update the class
                bool updateSuccess = await _exerciseRepository.UpdateTeacherClass(teacherId, className);

                if (updateSuccess)
                {
                    await SendResponseToSender(phoneNumber, $"✅ Class updated to '{className}' successfully!");
                    return "";
                }
                else
                {
                    await SendResponseToSender(phoneNumber, $"❌ Failed to update class. Please check the class name and try again.");
                    return "";
                }
            }


            else if (normalizedMessage.Contains("***"))
            {
                string instructionText = string.Empty;
                string exampleText = string.Empty;
                string classInstruction = string.Empty;

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
                        classInstruction = splitInstruction.ElementAtOrDefault(1)?.Trim() ?? string.Empty;
                    }

                    // Check if exampleText contains "###"
                    if (exampleText.Contains("###"))
                    {
                        var splitExample = exampleText.Split("###", StringSplitOptions.RemoveEmptyEntries);
                        exampleText = splitExample.ElementAtOrDefault(0)?.Trim() ?? string.Empty;
                        classInstruction = splitExample.ElementAtOrDefault(1)?.Trim() ?? string.Empty;
                    }
                    else if (string.IsNullOrEmpty(classInstruction) && messageParts.Length > 1) // Handle difficultyLevel from messageParts[1]
                    {
                        classInstruction = messageParts[1].Trim();
                    }
                }

                string midMessage = "\u200F מחולל 💡, יש למתין...⌛✨";

                await SendResponseToSender(phoneNumber, midMessage);
                //              await _whatsAppService.SendMessageToUser(phoneNumber, midMessage);
                //    difficultyLevel = "Easy";
                return await _whatsAppService.GetExercisesFromGPT(exampleText, teacherId, Constants.Teacher, classId, instructionText, userId, classInstruction); //TODO: change the 1, now its for test cos same user is teacher and student
            }


            //remind students
            else if (normalizedMessage.Contains("reminder"))
            {

                normalizedMessage = normalizedMessage.Normalize(NormalizationForm.FormC);
                string[] parts = normalizedMessage.Split(',');
                if (parts.Length != 3) return "The format should be:\nReminder, ClassName, Message to send";



                var classText = parts[1].Trim();
                string textToSend = parts[2].Trim();

                var studentsList = await _exerciseRepository.GetUsersByClassAsync(classText);

                foreach (var item in studentsList)
                {
                    //  textToSend = $"{item.FullName}\n איפה נעלמת?? \n {textToSend}";
                    string constructedTextToSend = @$"
🎉🥷✨ נינג'אדו כאן! ✨🥷🎉
{item.FullName}, בואו נצא לדרך! 🚀
{textToSend}
🎉🥷✨ בואו נראה מה אתם יודעים! ✨🥷🎉
";

                    await SendResponseToSender(item.PhoneNumber, constructedTextToSend);

                }
                return string.Empty;
            }

            else if (normalizedMessage.Contains("intro"))
            {

                normalizedMessage = normalizedMessage.Normalize(NormalizationForm.FormC);
                string[] parts = normalizedMessage.Split(',');
                if (parts.Length != 3) return "The format should be:\nReminder, ClassName, Message to send";



                var classText = parts[1].Trim();
                string textToSend = parts[2].Trim();

                var studentsList = await _exerciseRepository.GetUsersByClassAsync(classText);

                foreach (var item in studentsList)
                {
                    //  textToSend = $"{item.FullName}\n איפה נעלמת?? \n {textToSend}";
                    string constructedTextToSend = @$"
🥷✨ היי {item.FullName}! אני נינג'אדו, חבר מתמטי חדש! 📐😎  
תקוע פה בחידה! תעזור לי? 🤔🧩  
{textToSend}  
💥 בוא נפתור וננצח! 🚀🏆  
";


                    await SendResponseToSender(item.PhoneNumber, constructedTextToSend);

                }
                return string.Empty;
            }
            else
            {
                string exerciseText = "\u202A" + "*** 12 + _ = 14";
                string exerciseText2 = "\u202A" + "*** 12 * 6";
                string exerciseText3 = "\u202A" + "*** 15 - 7";

                return "\u200F *פקודה לא מוכרת.* \n" +
                       "\u200Fהנה הפקודות הזמינות שתוכל לשלוח כמורה:\n\n" +

                       "1. *lead*: \n" +
                       "\u200Fקבל את לוח התוצאות של הכיתה שלך.\n\n" +

                       "2. *status*: \n" +
                       "\u200Fקבל דו\"ח התקדמות מפורט של התלמידים ב-7 ימים האחרונים.\n\n" +

                       "3. *<הודעה עם ***>*:\n" +
                       "\u200Fצור 10 תרגילים חדשים. דוגמאות:\n" +
                       $"{exerciseText}\n" +
                       $"{exerciseText2}\n" +
                       $"{exerciseText3}\n" +
                       "כדי להביא דוגמה יש להשתמש ב-`//` ואז לכתוב דוגמה. לדוגמה: `// 2+2, 5+4`\n" +
                       "ניתן להוסיף: `###GRADE=5` כדי ליצור תרגילים לשכבה ספציפית. ציין את מספר השכבה, לדוגמה: `###GRADE=5`.\n\n" +

                       "4. *#update, [ClassName]*:\n" +
                       "\u200Fעדכן את הכיתה המשויכת אליך. לדוגמה: `#update, ה1`\n\n" +

                       "5. *reminder, ClassName, Message*:\n" +
                       "\u200Fשליחת תזכורת לתלמידי כיתה ספציפית. לדוגמה: `reminder, ה1, אל תשכחו לפתור תרגילים!`\n\n" +

                       "שיהיה בהצלחה! 💪";
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


        private async Task<string> TecherExercisesCreationHandle(int? userId, string? body)
        {
            Console.WriteLine($"approve teacher: user: {userId}, body: {body} ");
            if (userId != null)
            {
                var pendingExerciseId = await _exerciseRepository.GetPendingExerciseIdForUser(userId.Value);

                if (!string.IsNullOrEmpty(pendingExerciseId))
                {
                    var normalizedResponse = body?.Trim().ToLower();
                    var pendingExercises = await _exerciseRepository.GetPendingExercises(pendingExerciseId);

                    if (normalizedResponse == "כן" || normalizedResponse == "yes")
                    {
                        // User confirms the exercises
                        // Check if these exercises are for a whole grade (Grade set) and ClassId=0
                        if (pendingExercises.Grade.HasValue && pendingExercises.ClassId == 0)
                        {
                            // Get all classes for that grade
                            var classIds = await _exerciseRepository.GetClassesByGrade(pendingExercises.Grade.Value);

                            // Save exercises for all classes in one transaction
                            bool gradeInsertionSuccess = await _exerciseRepository.SaveExercisesForAllClassesInGrade(
                                pendingExercises.Response,
                                pendingExercises.CreatorUserId,
                                pendingExercises.CreatorRole,
                                classIds
                            );

                            if (!gradeInsertionSuccess)
                            {
                                return "שגיאה ביצירת תרגילים עבור הכיתות בדרגה המבוקשת.";
                            }

                            // If successfully inserted for all classes, just remove the pending exercises
                            await _exerciseRepository.DeletePendingExercises(pendingExerciseId);
                            await _exerciseRepository.DeletePendingExerciseIdForUser(userId.Value);
                            return "התרגילים נשמרו בהצלחה!";
                        }
                        else
                        {
                            // Normal flow: single class
                            bool res = await _exerciseRepository.SaveExercisesToDatabase(
                                pendingExercises.Response,
                                pendingExercises.CreatorUserId,
                                pendingExercises.CreatorRole,
                                pendingExercises.ClassId
                            );

                            if (!res)
                                return "שגיאה ביצירת תרגילים";

                            // Remove pending exercises
                            await _exerciseRepository.DeletePendingExercises(pendingExerciseId);
                            await _exerciseRepository.DeletePendingExerciseIdForUser(userId.Value);
                            return "התרגילים נשמרו בהצלחה!";
                        }
                    }
                    else if (normalizedResponse == "לא" || normalizedResponse == "no")
                    {
                        // User rejects the exercises
                        await _exerciseRepository.DeletePendingExercises(pendingExerciseId);
                        await _exerciseRepository.DeletePendingExerciseIdForUser(userId.Value);

                        return TextGeneratorFunctions.GetExerciseCreationText();
                    }
                    else
                    {
                        // Unexpected response
                        return "אנא כתוב 'כן' כדי לאשר ולשמור את התרגילים, או 'לא' כדי לבטל ולספק הוראות חדשות.";
                    }
                }
            }

            // If no userId or no pending exercises, return empty
            return string.Empty;
        }





    }

}
