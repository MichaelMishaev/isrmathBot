
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ExerciseRepository
{
    public ExerciseRepository(IConfiguration configuration) : base(configuration) { }

    public async Task SaveExercisesToDatabase(string jsonResponse, int teacherId)
    {
        // Parse the JSON response
        List<ExerciseModel> exercises = JsonConvert.DeserializeObject<List<ExerciseModel>>(jsonResponse);

        using (SqlConnection connection = GetConnection())
        {
            await connection.OpenAsync();

            foreach (var exercise in exercises)
            {
                string insertQuery = "INSERT INTO exercises (teacherId, exercise, correctAnswer, CreatedAt) VALUES (@TeacherId, @Exercise, @CorrectAnswer, @CreatedAt)";

                using (SqlCommand command = new SqlCommand(insertQuery, connection))
                {
                    // Set parameter values
                    command.Parameters.AddWithValue("@TeacherId", teacherId);
                    command.Parameters.AddWithValue("@Exercise", exercise.Exercise);
                    command.Parameters.AddWithValue("@CorrectAnswer", exercise.Answer);
                    command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);

                    // Execute the insert command
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
