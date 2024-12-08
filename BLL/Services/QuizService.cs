using BLL.Functions;
using BLL.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class QuizService
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly CommonFunctions _commonFunctions;

        public QuizService(ExerciseRepository exerciseRepository, CommonFunctions commonFunctions)
        {
            _exerciseRepository = exerciseRepository;
            _commonFunctions = commonFunctions;
        }

        public async Task<string> StartQuiz(int studentId, string phoneNumber)
        {
            // Initialize a quiz session
            int sessionId = await _exerciseRepository.CreateQuizSession(studentId);

            await _commonFunctions.SendResponseToSender(phoneNumber, TextGeneratorFunctions.GetQuizStartMessage());
            Thread.Sleep(3000);
            // Fetch the first question
            var exercise = await _exerciseRepository.GetNextUnassignedExercise(studentId);
            if (exercise.exercise == null || exercise.exercise == null)
            {
                await _exerciseRepository.EndQuizSession(sessionId, 0);
                return "No exercises available for the quiz. Try again later.";
            }

            // Save the first question for the session
            await _exerciseRepository.SaveCurrentQuizQuestion(sessionId, exercise.exercise.ExerciseId);

            // Send the first question
            return MathFunctions.FormatExerciseString(exercise.exercise.Exercise);
        }

        public async Task<string> HandleQuizAnswer(int studentId, int sessionId, string phoneNumber, string studentMessage)
        {
            int remainingTime = await _exerciseRepository.CalculateRemainingTime(sessionId);
            string formattedUserText = string.Empty;
            if (studentMessage.Trim().ToLower() == "skip")
            {
                var nextExercise = await _exerciseRepository.GetNextQuizQuestion(sessionId,studentId);
                if (nextExercise == null)
                {
                    return await EndQuiz(sessionId, phoneNumber);
                }

                await _exerciseRepository.SaveCurrentQuizQuestion(sessionId, nextExercise.ExerciseId);

                formattedUserText =  TextGeneratorFunctions.GetFormattedExerciseQuizMessage(MathFunctions.FormatExerciseString(nextExercise.Exercise), remainingTime);
                return formattedUserText;
            }

            var currentQuestion = await _exerciseRepository.GetCurrentQuizQuestion(sessionId);
            if (currentQuestion == null)
            {
                return "⚠️ No active question found. Please try again.";
            }

            bool isCorrect = currentQuestion.CorrectAnswer?.Trim() == studentMessage.Trim();
            //return TextGeneratorFunctions.GetRandomExerciseMessage(MathFunctions.FormatExerciseString(nextExercise.exercise.Exercise));
            string feedback = TextGeneratorFunctions.GetRandomCorrectOrIncorrectMessage(isCorrect);
                

            // Log the attempt
            await _exerciseRepository.LogQuizAttempt(sessionId, currentQuestion.ExerciseId, studentMessage, isCorrect);

            // Fetch the next question
            var nextQuestion = await _exerciseRepository.GetNextQuizQuestion(sessionId,studentId);
            if (nextQuestion == null || remainingTime == 0)
            {
                return await EndQuiz(sessionId, phoneNumber);
            }

            await _exerciseRepository.SaveCurrentQuizQuestion(sessionId, nextQuestion.ExerciseId);
         

             formattedUserText = TextGeneratorFunctions.GetFormattedExerciseQuizMessage(MathFunctions.FormatExerciseString(nextQuestion.Exercise), remainingTime);
            //await _commonFunctions.SendResponseToSender(phoneNumber, feedback);
            return $"{feedback}\n{formattedUserText}"; ;
        }



        private async Task<string> SendQuestion(string phoneNumber, ExerciseModel exercise)
        {
            string questionMessage = $"✏️ Question: {exercise.Exercise}\n⏳ Solve it quickly!";
    //        await SendResponseToSender(phoneNumber, questionMessage);
            return questionMessage;
        }

        public async Task<string> EndQuiz(int sessionId, string phoneNumber)
        {
            string summary = string.Empty;
            var sessionStats = await _exerciseRepository.GetQuizSessionStats(sessionId);
            if (sessionStats == null)
            {
                await _commonFunctions.SendResponseToSender("972544345287", "לא נוצר סטט לחידון");
                return "⚠️ לא הצלחנו ליצר חידון, כי כמעט סיימת ";
            }

                await _exerciseRepository.EndQuizSession(sessionId, sessionStats.Value.TotalCorrectAnswers);
            
            if (sessionStats.HasValue)
            {
                summary = $"🎉 **החידון הסתיים!** 🏆\n\n" +
                 $"📊 סיכום התוצאות:\n" +
                 $"🔢 סך הכל שאלות: {sessionStats.Value.TotalQuestions} ❓\n" +
                 $"✅ תשובות נכונות: {sessionStats.Value.TotalCorrectAnswers} \n" +
                 $"❌ תשובות לא נכונות: {sessionStats.Value.TotalQuestions - sessionStats.Value.TotalCorrectAnswers} \n\n" +
                 $"🔥 אתם פשוט אלופים! 💪\n" +
                 $"🌟 כל הכבוד על המאמץ! ✨\n";

            }
            return summary;
        }

    }
}
