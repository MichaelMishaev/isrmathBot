using BLL;
using BLL.Functions;
using BLL.Objects;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient; // MySQL client namespace
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

public class ExerciseRepository : DatabaseService
{
    private readonly CommonFunctions _commonFunctions;
    public ExerciseRepository(IConfiguration configuration, CommonFunctions commonFunctions) : base(configuration)
    {
        _commonFunctions = commonFunctions;
    }


    //********************
    // DAL/ExerciseRepository.cs
    





    //********************
    // DAL/ExerciseRepository.cs
    public async Task<UserTypeAndUserIdDTO?> GetUserRoleByPhoneNumber(string phoneNumber)
    {
        try
        {
            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
                SELECT 
                    u.UserId,
                    u.UserType,
                    CASE WHEN u.UserType = 'Teacher' THEN t.TeacherId
                         WHEN u.UserType = 'Parent' THEN p.ParentId
                         WHEN u.UserType = 'Student' THEN s.StudentId
                         ELSE NULL
                    END AS RoleSpecificId
                FROM users u
                LEFT JOIN teachers t ON u.UserId = t.UserId
                LEFT JOIN parents p ON u.UserId = p.UserId
                LEFT JOIN students s ON u.UserId = s.UserId
                WHERE u.PhoneNumber = @PhoneNumber";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var userRoleInfo = new UserTypeAndUserIdDTO
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                UserType = reader["UserType"].ToString(),
                                RoleSpecificId = reader["RoleSpecificId"] != DBNull.Value ? (int?)reader["RoleSpecificId"] : null
                            };

                            return userRoleInfo;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return null;
        }
    }

    public async Task<ExerciseModel> GetNextUnansweredExercise(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
        SELECT e.id, e.exercise, e.correctAnswer
        FROM exercises e
        INNER JOIN studentprogress sp ON sp.ExerciseId = e.id
        WHERE sp.StudentId = @StudentId AND sp.IsCorrect IS NULL
        ORDER BY sp.ProgressId ASC
        LIMIT 1";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ExerciseModel
                        {
                            ExerciseId = Convert.ToInt32(reader["id"]),
                            Exercise = reader["exercise"].ToString(),
                            CorrectAnswer = reader["correctAnswer"].ToString()
                        };
                    }
                }
            }
        }
        return null;
    }


    public async Task<ExerciseModel> GetLastExerciseForStudent(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT e.id, e.exercise, e.correctAnswer 
            FROM exercises e
            INNER JOIN studentprogress sp ON sp.ExerciseId = e.id
            WHERE sp.StudentId = @StudentId
            ORDER BY sp.ProgressId DESC
            LIMIT 1";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ExerciseModel
                        {
                            ExerciseId = Convert.ToInt32(reader["id"]),
                            Exercise = reader["exercise"].ToString(),
                            CorrectAnswer = reader["correctAnswer"].ToString()
                        };
                    }
                }
            }
        }
        return null;
    }

    //********************
    // DAL/ExerciseRepository.cs
    //********************
    // DAL/ExerciseRepository.cs
    public async Task UpdateStudentProgress(int studentId, int exerciseId, string studentAnswer, bool isCorrect)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query;
            if (isCorrect)
            {
                query = @"
                UPDATE studentprogress 
                SET StudentAnswer = @StudentAnswer, IsCorrect = @IsCorrect 
                WHERE StudentId = @StudentId AND ExerciseId = @ExerciseId";
            }
            else
            {
                query = @"
                UPDATE studentprogress 
                SET StudentAnswer = @StudentAnswer, IsCorrect = @IsCorrect, IncorrectAttempts = IncorrectAttempts + 1
                WHERE StudentId = @StudentId AND ExerciseId = @ExerciseId";
            }

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentAnswer", studentAnswer);
                command.Parameters.AddWithValue("@IsCorrect", isCorrect);
                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task UpdateStudentProgressTimestamp(int progressId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
    UPDATE studentprogress
    SET UpdatedAt = CURRENT_TIMESTAMP
    WHERE ProgressId = @ProgressId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProgressId", progressId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task UpdateGptHelpUsed(int studentId, int exerciseId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
        UPDATE studentprogress
        SET UsedGptHelp = 1
        WHERE StudentId = @StudentId AND ExerciseId = @ExerciseId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }


    //********************
    // DAL/ExerciseRepository.cs
    public async Task<int> GetIncorrectAttempts(int studentId, int exerciseId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT IncorrectAttempts
            FROM studentprogress
            WHERE StudentId = @StudentId AND ExerciseId = @ExerciseId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
    }


    public async Task<int> GetStudentIdByPhoneNumber(string phoneNumber)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT s.StudentId 
            FROM users u
            INNER JOIN students s ON s.UserId = u.UserId
            WHERE u.PhoneNumber = @PhoneNumber";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1; // Return -1 if the student is not found
            }
        }
    }


    //********************
    // DAL/ExerciseRepository.cs
    public async Task AddExerciseToStudentProgress(int studentId, int exerciseId, bool hasStarted)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            INSERT INTO studentprogress (StudentId, ExerciseId, HelpRequested, HasStarted, IncorrectAttempts)
            VALUES (@StudentId, @ExerciseId, 0, @HasStarted, 0)";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                command.Parameters.AddWithValue("@HasStarted", hasStarted ? 1 : 0);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> HasStudentStartedExercises(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT HasStarted 
            FROM studentprogress 
            WHERE StudentId = @StudentId 
            LIMIT 1";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                var result = await command.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
        }
    }

    public async Task<ExerciseModel> GetInProgressExercise(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
           SELECT e.id, e.exercise, e.correctAnswer, sp.updatedAt,sp.incorrectAttempts,sp.ProgressId,IsWaitingForHelp,e.QuestionType,ins.instructiontext,sp.UpdatedAt,e.answerOptions
           FROM exercises e
           LEFT JOIN Instructions as ins
	            ON ins.instructionId = e.instructionId
            INNER JOIN studentprogress sp ON sp.ExerciseId = e.id
            WHERE sp.StudentId = @StudentId 
            AND ((sp.StudentAnswer IS NULL OR sp.StudentAnswer = '') OR IsCorrect = 0)
            AND IsSkipped = 0
            AND status = 1
           ORDER BY sp.updatedAt ASC
            LIMIT 1";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ExerciseModel
                        {
                            ExerciseId = Convert.ToInt32(reader["id"]),
                            Exercise = reader["exercise"].ToString(),
                            CorrectAnswer = reader["correctAnswer"].ToString(),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updatedAt")), // Add updatedAt field
                            IncorrectAttempts = Convert.ToInt32(reader["incorrectAttempts"]),
                            ProgressId = Convert.ToInt32(reader["ProgressId"]),
                            IsWaitingForHelp = Convert.ToBoolean(reader["IsWaitingForHelp"]),
                            QuestionType = reader["QuestionType"].ToString(),
                            InstructionText = reader["instructiontext"].ToString(),
                            AnswerOptions = reader["answerOptions"] != DBNull.Value
                                                ? JsonConvert.DeserializeObject<List<AnswerOption>>(reader["answerOptions"].ToString())
                                                : null,
                        };
                    }
                }
            }
        }
        return null;
    }

    public async Task<int> GetExercisesLeftForStudent(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            // Updated query to calculate remaining exercises
            string remainingExercisesQuery = @"
SELECT COUNT(e.id) AS RemainingExercises
FROM exercises e
INNER JOIN students s ON e.classId = s.ClassId
LEFT JOIN studentprogress sp ON sp.ExerciseId = e.id AND sp.StudentId = s.StudentId
WHERE s.StudentId = @StudentId
  AND e.status = 1                  -- Active exercises
  AND e.DifficultyLevel = s.PreferredDifficultyLevel -- Match the preferred difficulty level
  AND (sp.IsCorrect IS NULL         -- Exclude completed or skipped exercises
       OR sp.IsCorrect = 0)
  AND (sp.IsSkipped IS NULL OR sp.IsSkipped = 0);";


            using (MySqlCommand command = new MySqlCommand(remainingExercisesQuery, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result); // Return the count of remaining exercises
            }
        }
    }


    public async Task UpdateIsWaitingForHelp(int studentId, int exerciseId, bool isWaiting)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            UPDATE studentprogress
            SET IsWaitingForHelp = @IsWaitingForHelp
            WHERE StudentId = @StudentId AND ExerciseId = @ExerciseId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                /////######### Set the IsWaitingForHelp parameter
                command.Parameters.AddWithValue("@IsWaitingForHelp", isWaiting ? 1 : 0);
                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }


    public async Task<List<(string PhoneNumber, string FullName)>> GetUsersByClassAsync(string inputParamClass)
    {
        try
        {
            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();
                //c.ClassName = @ClassName

                string query = @"
            SELECT DISTINCT u.PhoneNumber, u.FullName
            FROM students AS s
            INNER JOIN classes AS c ON c.classid = s.classId
            INNER JOIN users AS u ON u.UserId = s.UserId
            WHERE  u.Status = 1 AND c.ClassName = @ClassName
                                AND s.isToSendReminder = 1;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClassName", inputParamClass);

                    List<(string PhoneNumber, string FullName)> users = new List<(string PhoneNumber, string FullName)>();
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            users.Add((reader["PhoneNumber"].ToString(), reader["FullName"].ToString()));
                        }
                    }

                    return users;
                }
            }
        }
        catch (Exception e)
        {
            // Handle exception (e.g., log the error)
            return null;
        }
    }

    public async Task<List<(string PhoneNumber, string FullName)>> GetUsersByGradeAsync(string inputParamClass)
    {
        try
        {
            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                // Fetch the grade for the given class
                string gradeQuery = @"SELECT Grade FROM classes WHERE ClassName = @ClassName LIMIT 1;";
                using (MySqlCommand gradeCommand = new MySqlCommand(gradeQuery, connection))
                {
                    gradeCommand.Parameters.AddWithValue("@ClassName", inputParamClass);
                    object gradeResult = await gradeCommand.ExecuteScalarAsync();

                    if (gradeResult == null)
                        return new List<(string PhoneNumber, string FullName)>();

                    int grade = Convert.ToInt32(gradeResult);

                    // Fetch all students in the grade
                    string query = @"
                SELECT DISTINCT u.PhoneNumber, u.FullName
                FROM students AS s
                INNER JOIN classes AS c ON c.classid = s.classId
                INNER JOIN users AS u ON u.UserId = s.UserId
                WHERE u.Status = 1 AND c.Grade = @Grade
                      AND s.isToSendReminder = 1;";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Grade", grade);

                        List<(string PhoneNumber, string FullName)> users = new List<(string PhoneNumber, string FullName)>();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                users.Add((reader["PhoneNumber"].ToString(), reader["FullName"].ToString()));
                            }
                        }

                        return users;
                    }
                }
            }
        }
        catch (Exception e)
        {
            return new List<(string PhoneNumber, string FullName)>();
        }
    }


    public async Task<(string FullName, string ClassName)?> GetUserFullNameAndClassNameByStudentIdAsync(int studentId)
    {
        try
        {
            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
            SELECT u.fullName, cl.ClassName
            FROM students AS s
                    INNER JOIN users AS u ON s.UserId = u.UserId
                    INNER JOIN classes as cl ON cl.classId = s.ClassId
            WHERE s.StudentId = @StudentId AND u.Status = 1;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            string fullName = reader["FullName"].ToString();
                            string className = reader["ClassName"].ToString();
                            return (fullName, className);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception as necessary
            Console.WriteLine($"Error fetching data: {e.Message}");
        }

        return null; // Return null if not found or in case of an error
    }


    public async Task<string> GetUserFullNameByStudentIdAsync(int studentId)
    {
        try
        {
            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
                SELECT u.fullName, cl.ClassName
                FROM students AS s
                        INNER JOIN users AS u ON s.UserId = u.UserId
                        INNER JOIN classes as cl ON cl.classId = s.ClassId
                WHERE s.StudentId = @StudentId AND u.Status = 1;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return reader["FullName"].ToString();
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Log or handle the exception as necessary
            Console.WriteLine($"Error fetching FullName: {e.Message}");
        }

        return null; // Return null if not found or in case of an error
    }



    public async Task<(ExerciseModel? exercise, string? difficultyUpdate, string? changeType, string? instructionText)> GetNextUnassignedExercise(int studentId, int? lastDays = null)
    {
        try
        {
            string? difficultyUpdate = null;
            string? changeType = null;
            string? instructionText = null;

            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                // Approximately call difficulty update logic 1 in 3 times
                if (new Random().Next(5) == 0)
                {
                    //(difficultyUpdate, changeType) = await UpdateStudentDifficulty(connection, studentId);
                }

                // Query to fetch the next unassigned exercise
                string createdAtCondition = lastDays.HasValue
     ? "AND e.CreatedAt >= NOW() - INTERVAL @LastDays DAY"
     : "AND e.CreatedAt >= NOW() - INTERVAL 7 DAY";


                // Updated query with proper integration of the createdAtCondition
                string query = $@"
SELECT e.id, e.exercise, e.correctAnswer, e.DifficultyLevel, i.InstructionText,e.answerOptions,e.questionType
FROM exercises e
INNER JOIN students s ON e.classId = s.ClassId
LEFT JOIN instructions i ON e.InstructionId = i.InstructionId
WHERE s.StudentId = @StudentId
  AND e.id NOT IN (
      SELECT sp.ExerciseId
      FROM studentprogress sp
      WHERE sp.StudentId = @StudentId
        AND (sp.IsCorrect = 1 OR sp.IsSkipped = 1)
  )
  AND e.status = 1


  {createdAtCondition}
ORDER BY RAND()
LIMIT 1;";

                //if need to use diffLevel add :
                //AND e.DifficultyLevel = (
                //    SELECT PreferredDifficultyLevel
                //    FROM students
                //    WHERE StudentId = @StudentId
                //)
                //WHERE IS THE SPACE


                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);

                    if (lastDays.HasValue)
                    {
                        command.Parameters.AddWithValue("@LastDays", lastDays.Value);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            var exercise = new ExerciseModel
                            {
                                ExerciseId = Convert.ToInt32(reader["id"]),
                                Exercise = reader["exercise"].ToString(),
                                CorrectAnswer = reader["correctAnswer"].ToString(),
                                DifficultyLevel = reader["DifficultyLevel"].ToString(),
                                AnswerOptions = reader["answerOptions"] != DBNull.Value
                                                ? JsonConvert.DeserializeObject<List<AnswerOption>>(reader["answerOptions"].ToString())
                                                : null,
                                QuestionType = reader["questionType"].ToString(),
                                 
                            };

                            instructionText = reader["InstructionText"]?.ToString();
                            // Return both the exercise and any difficulty update info
                            return (exercise, difficultyUpdate, changeType, instructionText);
                        }
                    }
                }
            }

            // If no exercise is found, return null along with any difficulty update info
            return (null, difficultyUpdate, changeType, null);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNextUnassignedExercise: {ex.Message}");
            return (null, null, null, null);
        }
    }



    public async Task<string> GetInstructionFromDatabase(int instructionId)
    {
        if (instructionId <= 0)
            return null;

        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = "SELECT InstructionText FROM instructions WHERE InstructionId = @InstructionId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@InstructionId", instructionId);

                var result = await command.ExecuteScalarAsync();

                return result?.ToString();
            }
        }
    }




    private async Task<(string? updatedDifficulty, string? changeType)> UpdateStudentDifficulty(MySqlConnection connection, int studentId)
    {
        try
        {
            // Fetch the current difficulty level
            string currentDifficultyQuery = "SELECT PreferredDifficultyLevel FROM students WHERE StudentId = @StudentId;";
            string? currentDifficulty = null;

            using (MySqlCommand currentCommand = new MySqlCommand(currentDifficultyQuery, connection))
            {
                currentCommand.Parameters.AddWithValue("@StudentId", studentId);
                var result = await currentCommand.ExecuteScalarAsync();
                currentDifficulty = result?.ToString();
            }

            if (currentDifficulty == null)
            {
                // If no difficulty level is found, return null
                return (null, null);
            }

            // Downgrade logic: Check last 6 exercises for at least 2 incorrect answers
            string downgradeQuery = @"
        UPDATE students
        SET PreferredDifficultyLevel = CASE
            WHEN PreferredDifficultyLevel = 'Hard' THEN 'Medium'
            WHEN PreferredDifficultyLevel = 'Medium' THEN 'Easy'
            ELSE PreferredDifficultyLevel
        END,
        LastEvaluatedAt = NOW()
        WHERE StudentId = @StudentId
          AND (
              SELECT COUNT(*)
              FROM (
                  SELECT sp.IsCorrect
                  FROM studentprogress sp
                  WHERE sp.StudentId = @StudentId
                  ORDER BY sp.UpdatedAt DESC
                  LIMIT 6
              ) subquery
              WHERE subquery.IsCorrect = 0
          ) >= 2;";

            // Execute downgrade query
            int rowsAffectedByDowngrade;
            using (MySqlCommand downgradeCommand = new MySqlCommand(downgradeQuery, connection))
            {
                downgradeCommand.Parameters.AddWithValue("@StudentId", studentId);
                rowsAffectedByDowngrade = await downgradeCommand.ExecuteNonQueryAsync();
            }

            if (rowsAffectedByDowngrade > 0)
            {
                // Fetch the updated difficulty level after downgrade
                string updatedDifficultyQuery = "SELECT PreferredDifficultyLevel FROM students WHERE StudentId = @StudentId;";
                using (MySqlCommand updatedCommand = new MySqlCommand(updatedDifficultyQuery, connection))
                {
                    updatedCommand.Parameters.AddWithValue("@StudentId", studentId);
                    var updatedResult = await updatedCommand.ExecuteScalarAsync();
                    var updatedDifficulty = updatedResult?.ToString();
                    return (updatedDifficulty, "Downgraded");
                }
            }

            // Upgrade logic: Check last 15 exercises for at least 13 correct answers (no incorrect attempts)
            string upgradeQuery = @"
                   UPDATE students
            SET PreferredDifficultyLevel = CASE
                WHEN PreferredDifficultyLevel = 'Easy' THEN 'Medium'
                WHEN PreferredDifficultyLevel = 'Medium' THEN 'Hard'
                ELSE PreferredDifficultyLevel
            END,
            LastEvaluatedAt = NOW()
            WHERE StudentId = @StudentId
              AND (
                  SELECT COUNT(*)
                  FROM (
                      SELECT sp.IsCorrect, sp.IncorrectAttempts
                      FROM studentprogress sp
                      WHERE sp.StudentId = @StudentId
                      ORDER BY sp.UpdatedAt DESC
                      LIMIT 10
                  ) subquery
                  WHERE subquery.IsCorrect = 1
                     AND IncorrectAttempts = 0
              ) >= 8
              AND PreferredDifficultyLevel != 'Hard';";

            // Execute upgrade query
            int rowsAffectedByUpgrade;
            using (MySqlCommand upgradeCommand = new MySqlCommand(upgradeQuery, connection))
            {
                upgradeCommand.Parameters.AddWithValue("@StudentId", studentId);
                rowsAffectedByUpgrade = await upgradeCommand.ExecuteNonQueryAsync();
            }

            if (rowsAffectedByUpgrade > 0)
            {
                // Fetch the updated difficulty level after upgrade
                string updatedDifficultyQuery = "SELECT PreferredDifficultyLevel FROM students WHERE StudentId = @StudentId;";
                using (MySqlCommand updatedCommand = new MySqlCommand(updatedDifficultyQuery, connection))
                {
                    updatedCommand.Parameters.AddWithValue("@StudentId", studentId);
                    var updatedResult = await updatedCommand.ExecuteScalarAsync();
                    var updatedDifficulty = updatedResult?.ToString();
                    return (updatedDifficulty, "Upgraded");
                }
            }

            return (currentDifficulty, null); // No change in difficulty level
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateStudentDifficulty: {ex.Message}");
            return (null, null);
        }
    }


    public async Task<int> GetLastCorrectAnswers(int studentId)
    {
        try
        {

            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                // Combine initialization and query execution
                string query = @"
           SELECT COUNT(*) AS ConsecutiveCorrectAnswers
FROM (
    SELECT
        sp.IncorrectAttempts,
        SUM(CASE WHEN sp.IncorrectAttempts > 0 THEN 1 ELSE 0 END) 
        OVER (ORDER BY sp.UpdatedAt DESC) AS streakBreak
    FROM studentprogress sp
    WHERE sp.StudentId = @StudentId
   AND DATE(sp.UpdatedAt) = CURDATE()  -- Filter only today's records
    ORDER BY sp.UpdatedAt DESC
) AS sub
WHERE streakBreak = 0;
        ";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    var result = await command.ExecuteScalarAsync();

                    if (result != null && result != DBNull.Value)
                    {
                        return Convert.ToInt32(result);
                    }
                    return 0;
                }
            }
        }
        catch (Exception e)
        {

            throw e;
        }
    }


    public async Task<bool> isStudentAnswerFast(int studentId, int limit = 8)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT COUNT(*) 
            FROM studentprogress
            WHERE StudentId = @StudentId
              AND UpdatedAt >= NOW() - INTERVAL 1 MINUTE";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                int attemptsInLastMinute = Convert.ToInt32(await command.ExecuteScalarAsync());
                return attemptsInLastMinute > limit;
            }
        }
    }

    public async Task<string> GetHelpForExercise(int exerciseId, int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            // Retrieve the help content
            string query = @"
                            SELECT HelpContent
                            FROM exercises
                            WHERE id = @ExerciseId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                var result = await command.ExecuteScalarAsync();

                if (result != null)
                {
                    // Update the HelpRequested flag in studentprogress
                    await UpdateHelpRequested(studentId, exerciseId);
                    return result.ToString();
                }
                return null;
            }
        }
    }

    private async Task UpdateHelpRequested(int studentId, int exerciseId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string updateQuery = @"
        UPDATE studentprogress
        SET HelpRequested = 1
        WHERE StudentId = @StudentId AND ExerciseId = @ExerciseId";

            using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<bool> CheckIfStudentStarted(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = "SELECT COUNT(*) FROM studentprogress WHERE StudentId = @StudentId AND HasStarted = TRUE";
            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                var count = (long)await command.ExecuteScalarAsync();
                return count > 0;
            }
        }
    }

    //********************
    // DAL/ExerciseRepository.cs
    public async Task<List<StudentScore>> GetLeaderboardForClass(int classId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT u.FullName, COUNT(sp.IsCorrect) AS CorrectAnswers
            FROM students s
            INNER JOIN users u ON s.UserId = u.UserId
            INNER JOIN studentprogress sp ON s.StudentId = sp.StudentId
            WHERE s.ClassId = @ClassId AND sp.IsCorrect = 1
            GROUP BY s.StudentId
            ORDER BY CorrectAnswers DESC";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ClassId", classId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    List<StudentScore> leaderboard = new List<StudentScore>();
                    while (await reader.ReadAsync())
                    {
                        leaderboard.Add(new StudentScore
                        {
                            FullName = reader["FullName"].ToString(),
                            CorrectAnswers = Convert.ToInt32(reader["CorrectAnswers"])
                        });
                    }
                    return leaderboard;
                }
            }
        }
    }

    //********************
    // DAL/ExerciseRepository.cs
    public async Task<ClassProgress> GetClassProgress(int teacherId)
    {
        try
        {


            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = @"
            SELECT s.StudentId, u.FullName, SUM(sp.IsCorrect) AS CorrectAnswers, SUM(sp.IncorrectAttempts) AS WrongAnswers,SUM(sp.IsSkipped) AS skiped,
                    CASE 
                        WHEN SUM(sp.IsCorrect) + SUM(sp.IncorrectAttempts) > 0 
                        THEN ROUND((SUM(sp.IsCorrect) / (SUM(sp.IsCorrect) + SUM(sp.IncorrectAttempts)) * 100), 2)
                        ELSE NULL
                    END AS AverageCorrect
            FROM teachers t
            INNER JOIN students s ON s.ClassId = t.ClassId
            INNER JOIN users u ON s.UserId = u.UserId
            LEFT JOIN studentprogress sp ON s.StudentId = sp.StudentId AND sp.IsCorrect = 1
            AND sp.UpdatedAt >= NOW() - INTERVAL 7 DAY 
            WHERE t.TeacherId = @TeacherId
            GROUP BY s.StudentId
            ORDER BY CorrectAnswers DESC;";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TeacherId", teacherId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        List<StudentScore> classProgress = new List<StudentScore>();
                        while (await reader.ReadAsync())
                        {
                            classProgress.Add(new StudentScore
                            {
                                FullName = reader["FullName"].ToString(),
                                CorrectAnswers = reader["CorrectAnswers"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CorrectAnswers"]),
                                WrongAnswers = reader["WrongAnswers"] == DBNull.Value ? 0 : Convert.ToInt32(reader["WrongAnswers"]),
                                Skip = reader["skiped"] == DBNull.Value ? 0 : Convert.ToInt32(reader["skiped"]),
                                AverageCorrect = reader["AverageCorrect"] == DBNull.Value ? 0 : Convert.ToInt32(reader["AverageCorrect"])
                            });
                        }
                        return new ClassProgress { Students = classProgress };
                    }
                }
            }
        }
        catch (Exception e)
        {

            throw e;
        }
    }

    // DAL/TeacherRepository.cs
    public async Task<bool> UpdateTeacherClass(int teacherId, string className)
    {
        try
        {


            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                // Validate the className
                string validateQuery = @"
            SELECT c.ClassId
            FROM classes c
            WHERE c.ClassName = @ClassName;";

                int? classId = null;
                using (MySqlCommand validateCommand = new MySqlCommand(validateQuery, connection))
                {
                    validateCommand.Parameters.AddWithValue("@ClassName", className);
                    var result = await validateCommand.ExecuteScalarAsync();
                    if (result != null)
                    {
                        classId = Convert.ToInt32(result);
                    }
                }

                // If className is invalid, return false
                if (classId == null)
                {
                    return false;
                }

                // Update the teacher's ClassId
                string updateQuery = @"
            UPDATE teachers
            SET ClassId = @ClassId
            WHERE TeacherId = @TeacherId;";

                using (MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection))
                {
                    updateCommand.Parameters.AddWithValue("@ClassId", classId);
                    updateCommand.Parameters.AddWithValue("@TeacherId", teacherId);

                    int rowsAffected = await updateCommand.ExecuteNonQueryAsync();
                    return rowsAffected > 0; // Return true if the update was successful
                }
            }
        }
        catch (Exception e)
        {

            throw e;
        }
    }




    //********************
    // DAL/ExerciseRepository.cs
    public async Task<List<StudentScore>> GetLeaderboardForClassByTeacherId(int teacherId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT u.FullName, SUM(sp.IsCorrect) AS CorrectAnswers, SUM(sp.IncorrectAttempts) AS WrongAnswers
            FROM teachers t
            INNER JOIN students s ON s.ClassId = t.ClassId
            INNER JOIN users u ON s.UserId = u.UserId
            INNER JOIN studentprogress sp ON s.StudentId = sp.StudentId
            WHERE t.TeacherId = @TeacherId AND sp.IsCorrect = 1
            GROUP BY s.StudentId
            ORDER BY CorrectAnswers DESC";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@TeacherId", teacherId);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    List<StudentScore> leaderboard = new List<StudentScore>();
                    while (await reader.ReadAsync())
                    {
                        leaderboard.Add(new StudentScore
                        {
                            FullName = reader["FullName"].ToString(),
                            CorrectAnswers = Convert.ToInt32(reader["CorrectAnswers"]),
                            WrongAnswers = Convert.ToInt32(reader["WrongAnswers"])
                        });
                    }
                    return leaderboard;
                }
            }
        }
    }

    public async Task<(int TeacherId, int ClassName)> GetTeacherIdByPhoneNumber(string phoneNumber)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
        SELECT t.TeacherId,  t.classId
        FROM users u
        INNER JOIN teachers t ON u.UserId = t.UserId
        INNER JOIN classes c ON c.ClassId = t.ClassId
        WHERE u.PhoneNumber = @PhoneNumber";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        int teacherId = reader.GetInt32("TeacherId");
                        int classId = reader.GetInt32("classId");
                        return (teacherId, classId);
                    }
                    else
                    {
                        return (-1, -1); // Return -1 and null if the teacher is not found
                    }
                }
            }
        }
    }

    public async Task SkipCurrentExercise(int progressId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            UPDATE studentprogress
            SET IsSkipped = 1
            WHERE ProgressId = @ProgressId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ProgressId", progressId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }




    //********************
    public async Task<int> GetParentIdByPhoneNumber(string phoneNumber)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT p.ParentId
            FROM users u
            INNER JOIN parents p ON u.UserId = p.UserId
            WHERE u.PhoneNumber = @PhoneNumber";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PhoneNumber", phoneNumber);

                var result = await command.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : -1; // Return -1 if the parent is not found
            }
        }
    }



    public async Task<int> GetExercisesSolvedToday(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string countQuery = @"
        SELECT COUNT(*) 
        FROM studentprogress 
        WHERE StudentId = @StudentId 
        AND IsCorrect = 1 
        AND DATE(UpdatedAt) = CURDATE()";

            using (MySqlCommand countCommand = new MySqlCommand(countQuery, connection))
            {
                countCommand.Parameters.AddWithValue("@StudentId", studentId);

                var countResult = await countCommand.ExecuteScalarAsync();
                return Convert.ToInt32(countResult);
            }
        }
    }
    //********************
    // DAL/ExerciseRepository.cs
    public async Task IncrementWrongAnswerCount(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
                    UPDATE students
                    SET WrongAnswerCount = WrongAnswerCount + 1
                    WHERE StudentId = @StudentId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    //********************
    public async Task ResetWrongAnswerCount(int studentId)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
        UPDATE students
        SET WrongAnswerCount = 0
        WHERE StudentId = @StudentId";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }


    /// <summary>
    /// panding teachers exercise logics
    /// </summary>
    /// 


    public async Task SavePendingExercises(string pendingId, string response, int creatorUserId, string creatorRole, int classId, int? grade = null)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    // Delete existing pending exercises for the creator
                    await DeletePendingExercisesByCreatorUserId(creatorUserId, connection, transaction);

                    // Insert new PendingExercises first
                    var queryPendingExercises = @"
                INSERT INTO PendingExercises (PendingId, Response, CreatorUserId, CreatorRole, ClassId, Grade) 
                VALUES (@PendingId, @Response, @CreatorUserId, @CreatorRole, @ClassId, @Grade)";

                    using (var command = new MySqlCommand(queryPendingExercises, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@PendingId", pendingId);
                        command.Parameters.AddWithValue("@Response", response);
                        command.Parameters.AddWithValue("@CreatorUserId", creatorUserId);
                        command.Parameters.AddWithValue("@CreatorRole", creatorRole);
                        command.Parameters.AddWithValue("@ClassId", classId);
                        command.Parameters.AddWithValue("@Grade", grade as object ?? DBNull.Value);
                        await command.ExecuteNonQueryAsync();
                    }

                    // Then insert or update UserPendingExercises
                    var queryUserPending = @"INSERT INTO UserPendingExercises (UserId, PendingId) 
                                         VALUES (@UserId, @PendingId) 
                                         ON DUPLICATE KEY UPDATE PendingId = @PendingId";
                    using (var command = new MySqlCommand(queryUserPending, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@UserId", creatorUserId);
                        command.Parameters.AddWithValue("@PendingId", pendingId);
                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();

                }
            }
        }
    }


    public async Task<List<int>> GetClassesByGrade(int grade)
    {
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = "SELECT ClassId FROM classes WHERE Grade = @Grade";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Grade", grade);
                using (var reader = await command.ExecuteReaderAsync())
                {
                    List<int> classIds = new List<int>();
                    while (await reader.ReadAsync())
                    {
                        classIds.Add(Convert.ToInt32(reader["ClassId"]));
                    }
                    return classIds;
                }
            }
        }
    }

    public async Task<bool> SaveExercisesToDatabase(string jsonResponse, int creatorUserId, string creatorRole, int classId)
    {
        try
        {
            // Step 1: Handle double-serialized JSON
            //     string innerJson = JsonConvert.DeserializeObject<string>(jsonResponse);
            List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(jsonResponse);

            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                foreach (var exercise in exercises)
                {
                    string insertQuery = @"
                INSERT INTO exercises 
                (CreatedByUserId, CreatedByRole, exercise, correctAnswer, HelpContent, CreatedAt, classId, DifficultyLevel, QuestionType, InstructionId, AnswerOptions) 
                VALUES 
                (@CreatedByUserId, @CreatedByRole, @Exercise, @CorrectAnswer, @HelpContent, @CreatedAt, @ClassId, @DifficultyLevel, @QuestionType, @InstructionId, @AnswerOptions)";


                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@CreatedByUserId", creatorUserId);
                        command.Parameters.AddWithValue("@CreatedByRole", creatorRole);
                        command.Parameters.AddWithValue("@Exercise", exercise.Exercise);
                        command.Parameters.AddWithValue("@CorrectAnswer", exercise.CorrectAnswer);
                        command.Parameters.AddWithValue("@HelpContent", exercise.Hint);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@ClassId", classId);
                        command.Parameters.AddWithValue("@DifficultyLevel", exercise.DifficultyLevel ?? "Easy");
                        command.Parameters.AddWithValue("@QuestionType", exercise.QuestionType ?? "OpenAnswer");
                        command.Parameters.AddWithValue("@InstructionId", exercise.InstructionId);
                        if (exercise.AnswerOptions != null && exercise.AnswerOptions.Any())
                        {
                            command.Parameters.AddWithValue("@AnswerOptions", JsonConvert.SerializeObject(exercise.AnswerOptions));
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@AnswerOptions", DBNull.Value);
                        }

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            await _commonFunctions.SendResponseToSender("972544345287", $"error in SaveExercisesToDatabase {e.Message}");
            return false;
        }
    }

    public async Task<bool> SaveExercisesForAllClassesInGrade(string jsonResponse, int creatorUserId, string creatorRole, List<int> classIds)
    {
        try
        {
            // Double-deserialize to get actual exercise list
            //string innerJson = JsonConvert.DeserializeObject<string>(jsonResponse);
            List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(jsonResponse);

            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();
                using (MySqlTransaction transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (int classId in classIds)
                        {
                            foreach (var exercise in exercises)
                            {
                                string insertQuery = @"
                            INSERT INTO exercises 
                                (CreatedByUserId, CreatedByRole, exercise, correctAnswer, HelpContent, CreatedAt, classId, DifficultyLevel, QuestionType,AnswerOptions) 
                            VALUES 
                                (@CreatedByUserId, @CreatedByRole, @Exercise, @CorrectAnswer, @HelpContent, @CreatedAt, @ClassId, @DifficultyLevel, @QuestionType,@AnswerOptions)";

                                using (MySqlCommand command = new MySqlCommand(insertQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@CreatedByUserId", creatorUserId);
                                    command.Parameters.AddWithValue("@CreatedByRole", creatorRole);
                                    command.Parameters.AddWithValue("@Exercise", exercise.Exercise);
                                    command.Parameters.AddWithValue("@CorrectAnswer", exercise.CorrectAnswer);
                                    command.Parameters.AddWithValue("@HelpContent", exercise.Hint);
                                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                                    command.Parameters.AddWithValue("@ClassId", classId);
                                    command.Parameters.AddWithValue("@DifficultyLevel", exercise.DifficultyLevel ?? "Easy");
                                    command.Parameters.AddWithValue("@QuestionType", exercise.QuestionType ?? "OpenAnswer");
                                    if (exercise.AnswerOptions != null && exercise.AnswerOptions.Any())
                                    {
                                        command.Parameters.AddWithValue("@AnswerOptions", JsonConvert.SerializeObject(exercise.AnswerOptions));
                                    }
                                    else
                                    {
                                        command.Parameters.AddWithValue("@AnswerOptions", DBNull.Value);
                                    }

                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        await transaction.RollbackAsync();
                        return false;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return false;
        }
    }



    public async Task DeletePendingExercisesByCreatorUserId(int creatorUserId, MySqlConnection connection, MySqlTransaction transaction)
    {
        string pendingIdQuery = "SELECT PendingId FROM UserPendingExercises WHERE UserId = @CreatorUserId";
        string? pendingId = null;

        using (var command = new MySqlCommand(pendingIdQuery, connection, transaction))
        {
            command.Parameters.AddWithValue("@CreatorUserId", creatorUserId);
            var result = await command.ExecuteScalarAsync();
            pendingId = result as string;
        }

        if (!string.IsNullOrEmpty(pendingId))
        {
            var deletePendingQuery = "DELETE FROM PendingExercises WHERE PendingId = @PendingId";
            using (var command = new MySqlCommand(deletePendingQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@PendingId", pendingId);
                await command.ExecuteNonQueryAsync();
            }

            var deleteUserPendingQuery = "DELETE FROM UserPendingExercises WHERE UserId = @CreatorUserId";
            using (var command = new MySqlCommand(deleteUserPendingQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@CreatorUserId", creatorUserId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<PendingExerciseModel> GetPendingExercises(string pendingId)
    {
        var query = "SELECT * FROM PendingExercises WHERE PendingId = @PendingId";

        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@PendingId", pendingId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new PendingExerciseModel
                        {
                            PendingId = reader["PendingId"].ToString(),
                            Response = reader["Response"].ToString(),
                            CreatorUserId = Convert.ToInt32(reader["CreatorUserId"]),
                            CreatorRole = reader["CreatorRole"].ToString(),
                            ClassId = Convert.ToInt32(reader["ClassId"]),
                            Grade = reader["Grade"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Grade"])
                        };
                    }
                }
            }
        }

        return null;
    }

    public async Task DeletePendingExercises(string pendingId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var transaction = await connection.BeginTransactionAsync())
            {
                try
                {
                    var deletePendingQuery = "DELETE FROM PendingExercises WHERE PendingId = @PendingId";
                    using (var command = new MySqlCommand(deletePendingQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@PendingId", pendingId);
                        await command.ExecuteNonQueryAsync();
                    }

                    var deleteUserPendingQuery = "DELETE FROM UserPendingExercises WHERE PendingId = @PendingId";
                    using (var command = new MySqlCommand(deleteUserPendingQuery, connection, transaction))
                    {
                        command.Parameters.AddWithValue("@PendingId", pendingId);
                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
    }

    public async Task SavePendingExerciseIdForUser(int userId, string pendingId)
    {
        var query = @"INSERT INTO UserPendingExercises (UserId, PendingId) 
                      VALUES (@UserId, @PendingId) 
                      ON DUPLICATE KEY UPDATE PendingId = @PendingId";

        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@PendingId", pendingId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<string?> GetPendingExerciseIdForUser(int userId)
    {
        var query = "SELECT PendingId FROM UserPendingExercises WHERE UserId = @UserId";

        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                var result = await command.ExecuteScalarAsync();
                return result as string;
            }
        }
    }

    public async Task DeletePendingExerciseIdForUser(int userId)
    {
        var query = "DELETE FROM UserPendingExercises WHERE UserId = @UserId";

        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }


    //#################################################################
    //############################## QUEEZ ############################
    //#################################################################

    public async Task<int> CreateQuizSession(int studentId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"
        INSERT INTO quiz_sessions (StudentId, StartTime, IsActive)
        VALUES (@StudentId, NOW(), TRUE);
        SELECT LAST_INSERT_ID();";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }
    }

    public async Task<(int TotalQuestions, int TotalCorrectAnswers)?> GetQuizSessionStats(int sessionId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"
        SELECT 
            COUNT(*) AS TotalQuestions,
            SUM(IsCorrect) AS TotalCorrectAnswers
        FROM quiz_attempts
        WHERE SessionId = @SessionId;";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return (
                            reader.GetInt32("TotalQuestions"),
                            reader.IsDBNull("TotalCorrectAnswers") ? 0 : reader.GetInt32("TotalCorrectAnswers")
                        );
                    }
                }
            }
        }
        return null;
    }

    public async Task EndQuizSession(int sessionId, int totalCorrectAnswers)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"
        UPDATE quiz_sessions
        SET IsActive = FALSE, EndTime = NOW(), TotalCorrectAnswers = @TotalCorrectAnswers
        WHERE SessionId = @SessionId;";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);
                command.Parameters.AddWithValue("@TotalCorrectAnswers", totalCorrectAnswers);
                await command.ExecuteNonQueryAsync();
            }
        }
    }



    public async Task<ExerciseModel?> GetNextQuizQuestion(int sessionId, int studentId, int? lastDays = null)
    {
        try
        {
            using (var connection = GetConnection())
            {
                await connection.OpenAsync();

                // Determine the createdAt condition
                string createdAtCondition = lastDays.HasValue
                    ? "AND e.CreatedAt >= NOW() - INTERVAL @LastDays DAY"
                    : "AND e.CreatedAt >= NOW() - INTERVAL 7 DAY";

                // Query to fetch the next quiz question, incorporating additional filtering logic
                string query = $@"
            SELECT e.id, e.exercise, e.correctAnswer, e.DifficultyLevel, e.*
            FROM exercises e
            INNER JOIN students s ON e.classId = s.ClassId
            WHERE s.StudentId = @StudentId
              AND e.id NOT IN (
                  SELECT ExerciseId
                  FROM quiz_attempts
                  WHERE SessionId = @SessionId
              )
              AND e.status = 1
              AND e.DifficultyLevel = (
                  SELECT PreferredDifficultyLevel
                  FROM students
                  WHERE StudentId = @StudentId
              )
              {createdAtCondition}
            ORDER BY RAND()
            LIMIT 1;";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    command.Parameters.AddWithValue("@SessionId", sessionId);

                    if (lastDays.HasValue)
                    {
                        command.Parameters.AddWithValue("@LastDays", lastDays.Value);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new ExerciseModel
                            {
                                ExerciseId = Convert.ToInt32(reader["id"]),
                                Exercise = reader["exercise"].ToString(),
                                CorrectAnswer = reader["correctAnswer"].ToString(),
                                DifficultyLevel = reader["DifficultyLevel"].ToString()
                            };
                        }
                    }
                }
            }

            // Return null if no question is found
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetNextQuizQuestion: {ex.Message}");
            return null;
        }
    }



    public async Task LogQuizAttempt(int sessionId, int exerciseId, string studentAnswer, bool isCorrect)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"
        INSERT INTO quiz_attempts (SessionId, ExerciseId, StudentAnswer, IsCorrect, AttemptedAt)
        VALUES (@SessionId, @ExerciseId, @StudentAnswer, @IsCorrect, NOW())";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                command.Parameters.AddWithValue("@StudentAnswer", studentAnswer);
                command.Parameters.AddWithValue("@IsCorrect", isCorrect);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task SaveCurrentQuizQuestion(int sessionId, int exerciseId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"
            UPDATE quiz_sessions
            SET CurrentExerciseId = @ExerciseId
            WHERE SessionId = @SessionId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ExerciseId", exerciseId);
                command.Parameters.AddWithValue("@SessionId", sessionId);
                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<int?> GetActiveQuizSession(int studentId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();

            // Query to get the latest session
            string fetchLatestSessionIdQuery = @"
        SELECT SessionId
        FROM quiz_sessions
        WHERE StudentId = @StudentId AND IsActive = TRUE
        ORDER BY StartTime DESC
        LIMIT 1;";

            // Query to deactivate older sessions
            string deactivateOlderSessionsQuery = @"
        UPDATE quiz_sessions
        SET IsActive = FALSE
        WHERE StudentId = @StudentId AND IsActive = TRUE AND SessionId != @LatestSessionId;";

            using (var command = new MySqlCommand(fetchLatestSessionIdQuery, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                var result = await command.ExecuteScalarAsync();
                int? latestSessionId = result != null ? Convert.ToInt32(result) : (int?)null;

                if (latestSessionId.HasValue)
                {
                    // Deactivate older sessions
                    using (var updateCommand = new MySqlCommand(deactivateOlderSessionsQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@StudentId", studentId);
                        updateCommand.Parameters.AddWithValue("@LatestSessionId", latestSessionId.Value);
                        await updateCommand.ExecuteNonQueryAsync();
                    }
                }

                return latestSessionId;
            }
        }
    }


    public async Task<ExerciseModel?> GetCurrentQuizQuestion(int sessionId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"
            SELECT e.id, e.exercise, e.correctAnswer
            FROM exercises e
            INNER JOIN quiz_sessions qs ON qs.CurrentExerciseId = e.id
            WHERE qs.SessionId = @SessionId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new ExerciseModel
                        {
                            ExerciseId = reader.GetInt32("id"),
                            Exercise = reader.GetString("exercise"),
                            CorrectAnswer = reader.GetString("correctAnswer")
                        };
                    }
                }
            }
        }
        return null;
    }


    public async Task<int> CalculateRemainingTime(int sessionId)
    {
        using (var connection = GetConnection())
        {
            await connection.OpenAsync();
            string query = @"SELECT TIMESTAMPDIFF(SECOND, StartTime, NOW()) AS ElapsedTime FROM quiz_sessions WHERE SessionId = @SessionId";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@SessionId", sessionId);
                var result = await command.ExecuteScalarAsync();
                int elapsedTime = result != null ? Convert.ToInt32(result) : 0;
                return Math.Max(60 - elapsedTime, 0); // Ensure non-negative remaining time
            }
        }
    }


    public async Task<List<ClassAndSchoolDTO>> GetAllClassData()
    {
        var classDataList = new List<ClassAndSchoolDTO>();

        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
           SELECT 
    c.ClassId,
    c.ClassName,
    c.Grade,
    sch.SchoolName,
    sch.Address,
    COUNT(s.StudentId) AS StudentCount
FROM 
    classes c
LEFT JOIN 
    students s ON c.ClassId = s.ClassId
LEFT JOIN 
    schools sch ON c.SchoolId = sch.SchoolId
GROUP BY 
    c.ClassId, c.ClassName, c.Grade, sch.SchoolName, sch.Address
ORDER BY 
    c.ClassName;";

            using (MySqlCommand command = new MySqlCommand(query, connection))
            {
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var classData = new ClassAndSchoolDTO
                        {
                            ClassId = reader.GetInt32("ClassId"),
                            ClassName = reader.GetString("ClassName"),
                            Grade = reader.IsDBNull(reader.GetOrdinal("Grade")) ? null : (int?)reader.GetInt32("Grade"),
                            SchoolName = reader.GetString("SchoolName"),
                            Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? null : reader.GetString("Address"),
                            StudCount = reader.IsDBNull(reader.GetOrdinal("StudentCount")) ? null : (int?)reader.GetInt32("StudentCount")
                        };
                        classDataList.Add(classData);
                    }
                }
            }
        }

        return classDataList;
    }


    ///////////////////////////////////////////////////////////////////


    public class ClassProgress
    {
        public List<StudentScore> Students { get; set; }
    }

    public class StudentScore
    {
        public string FullName { get; set; }
        public int CorrectAnswers { get; set; }
        public int WrongAnswers { get; set; }
        public int Skip { get; set; }
        public int AverageCorrect { get; set; }
    }


    public class PendingExerciseModel
    {
        public string PendingId { get; set; }

        public string Response { get; set; }
        public int CreatorUserId { get; set; }
        public string CreatorRole { get; set; }
        public int ClassId { get; set; }
        public int? Grade { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    // Override the GetConnection method to use MySqlConnection

}
