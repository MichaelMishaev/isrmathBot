﻿using BLL;
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
    public ExerciseRepository(IConfiguration configuration) : base(configuration) { }

    //********************
    // DAL/ExerciseRepository.cs
    public async Task<bool> SaveExercisesToDatabase(string jsonResponse, int creatorUserId, string creatorRole, int classId)
    {
        try
        {
            List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(jsonResponse);

            using (MySqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                foreach (var exercise in exercises)
                {
                    string insertQuery = @"INSERT INTO exercises 
                    (CreatedByUserId, CreatedByRole, exercise, correctAnswer, HelpContent, CreatedAt, classId) 
                    VALUES (@CreatedByUserId, @CreatedByRole, @Exercise, @CorrectAnswer, @HelpContent, @CreatedAt, @ClassId)";

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@CreatedByUserId", creatorUserId);
                        command.Parameters.AddWithValue("@CreatedByRole", creatorRole);
                        command.Parameters.AddWithValue("@Exercise", exercise.Exercise);
                        command.Parameters.AddWithValue("@CorrectAnswer", exercise.CorrectAnswer);
                        command.Parameters.AddWithValue("@HelpContent", exercise.Hint);
                        command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);
                        command.Parameters.AddWithValue("@ClassId", classId);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return false;
        }
    }




    //********************
    // DAL/ExerciseRepository.cs
    public async Task<UserTypeAndUserIdDTO> GetUserRoleByPhoneNumber(string phoneNumber)
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
            SELECT e.id, e.exercise, e.correctAnswer, sp.updatedAt,sp.incorrectAttempts,sp.ProgressId
            FROM exercises e
            INNER JOIN studentprogress sp ON sp.ExerciseId = e.id
            WHERE sp.StudentId = @StudentId 
            AND ((sp.StudentAnswer IS NULL OR sp.StudentAnswer = '') OR IsCorrect = 0)
            AND IsSkipped = 0
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
                            CorrectAnswer = reader["correctAnswer"].ToString(),
                            UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updatedAt")), // Add updatedAt field
                            IncorrectAttempts = Convert.ToInt32(reader["incorrectAttempts"]),
                            ProgressId = Convert.ToInt32(reader["ProgressId"])
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
          AND e.status = 1               -- Active exercises
          AND (sp.IsCorrect IS NULL      -- Exclude completed or skipped exercises
               OR sp.IsCorrect = 0)
          AND (sp.IsSkipped IS NULL OR sp.IsSkipped = 0)";

            using (MySqlCommand command = new MySqlCommand(remainingExercisesQuery, connection))
            {
                command.Parameters.AddWithValue("@StudentId", studentId);

                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result); // Return the count of remaining exercises
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
            WHERE  u.Status = 1;";

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

    public async Task<ExerciseModel> GetNextUnassignedExercise(int studentId)
    {
        try
        {


        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

                string query = @"
        SELECT e.id, e.exercise, e.correctAnswer,e.*
FROM exercises e
INNER JOIN students s ON e.classId = s.ClassId
WHERE s.StudentId = @StudentId
AND e.id NOT IN (
    SELECT sp.ExerciseId
    FROM studentprogress sp
    WHERE sp.StudentId = @StudentId AND sp.IsCorrect = 1
          OR isSkipped = 1
)

AND e.status = 1
ORDER BY RAND()
LIMIT 1;";

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
        catch (Exception e)
        {

            return null;
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
        using (MySqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            string query = @"
            SELECT s.StudentId, u.FullName, COUNT(sp.IsCorrect) AS CorrectAnswers, COUNT(sp.IncorrectAttempts) AS WrongAnswers
            FROM teachers t
            INNER JOIN students s ON s.ClassId = t.ClassId
            INNER JOIN users u ON s.UserId = u.UserId
            LEFT JOIN studentprogress sp ON s.StudentId = sp.StudentId AND sp.IsCorrect = 1
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
                            CorrectAnswers = Convert.ToInt32(reader["CorrectAnswers"]),
                            WrongAnswers = Convert.ToInt32(reader["WrongAnswers"])
                        });
                    }
                    return new ClassProgress { Students = classProgress };
                }
            }
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
            SELECT u.FullName, COUNT(sp.IsCorrect) AS CorrectAnswers
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
                            CorrectAnswers = Convert.ToInt32(reader["CorrectAnswers"])
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



    public class ClassProgress
    {
        public List<StudentScore> Students { get; set; }
    }

    public class StudentScore
    {
        public string FullName { get; set; }
        public int CorrectAnswers { get; set; }
        public int  WrongAnswers { get; set; }
    }


    // Override the GetConnection method to use MySqlConnection

}