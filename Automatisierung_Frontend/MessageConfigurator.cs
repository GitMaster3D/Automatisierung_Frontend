using Api_Connection;
using Newtonsoft.Json;
using System.Net;

namespace Automatisierung_Frontend
{
    public class MessageConfigurator
    {
        public static async Task<bool> UpdateWateringData(int targetPump, WateringData wateringData)
        {
            wateringData.TargetPump = targetPump;

            var response = await API_Connection.Post("/setprogram", wateringData);

            return response.statusCode == HttpStatusCode.OK;
        }

        public static async void WaterImmediate(int targetPump, WaterImmediateData data)
        {
            data.TargetPump = targetPump;

            Console.WriteLine(await API_Connection.Post("/waterimmediate", data));
            
        }
    }


    public class WaterImmediateData
    {
        public float Time { get; private set; }

        public int TargetPump;

        public void SetTime(float time)
        {
            Time = time;
        }
    }


public class WateringData
    {
        public float TargetMoisture_Percentage
        {
            get => TargetMoisturePercentage;
            set
            {
                TargetMoisturePercentage = value;
                if (TargetMoisturePercentage > 100) TargetMoisturePercentage = 100;
                if (TargetMoisturePercentage < 0) TargetMoisturePercentage = 0;
            }
        }
        private float TargetMoisturePercentage;

	    public float TargetDailyWater_Liters { get; set; }


        [JsonConverter(typeof(CustomTimeSpanListConverter), @"hh\:mm")]
        public List<TimeSpan?> DailyWateringTimes { get; set; } = new();

        public WateringMode Mode { get; set; } = WateringMode.Undefined;

	    internal int TargetPump;

        public void RemoveWateringTime(TimeSpan? time)
        {
            DailyWateringTimes.Remove(time);
		}

        public void AddWateringTime(TimeSpan? time)
        {
            DailyWateringTimes.Add(time);
            DailyWateringTimes.Sort();
        }

        /// <summary>
        /// Check if the Watering Data is validly configured
        /// </summary>
        public bool ValidateData()
        {
            return Mode != WateringMode.Undefined
                && ((Mode == WateringMode.Moisture) || (Mode == WateringMode.Amount && (DailyWateringTimes.Count > 0)));
        }


        /// <summary>
        /// Sets the times on wich water can be given at any day
        /// </summary>
        public void SetDailyWateringTimes(TimeSpan?[] dailyWateringTimes)
        {
            DailyWateringTimes = dailyWateringTimes.ToList();
        }

        /// <summary>
        /// Sets the watering mode to water daily a given amount
        /// </summary>
        public void UseDailyAmount(float litersPerDay)
        {
            TargetDailyWater_Liters = litersPerDay;
            Mode = WateringMode.Amount;
        }

        /// <summary>
        /// Sets the watering mode to hold the given Percentage constantly
        /// </summary>
        public void UsePercentage(float targetMoisture)
        {
            TargetMoisture_Percentage = targetMoisture;
            Mode = WateringMode.Moisture;
        }



        public enum WateringMode
        {
            Undefined = 0,
            Moisture = 1,
            Amount = 2
        }
    }
}


public class CustomTimeSpanListConverter : JsonConverter<List<TimeSpan?>>
{
    private readonly string _format;

    // Constructor to allow passing a custom format
    public CustomTimeSpanListConverter(string format = @"hh\:mm\:ss")
    {
        _format = format;
    }

    public override void WriteJson(JsonWriter writer, List<TimeSpan?> value, JsonSerializer serializer)
    {
        // Write the list of TimeSpan as an array of strings
        writer.WriteStartArray();
        foreach (var timeSpan in value)
        {
            writer.WriteValue(timeSpan.Value.ToString(_format));
        }
        writer.WriteEndArray();
    }

    public override List<TimeSpan?> ReadJson(JsonReader reader, Type objectType, List<TimeSpan?> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        // Read the JSON array of strings into a list of TimeSpan
        if (reader.TokenType == JsonToken.StartArray)
        {
            var timeSpanList = new List<TimeSpan?>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                {
                    break;
                }

                if (reader.TokenType == JsonToken.String && reader.Value is string timeSpanString)
                {
                    timeSpanList.Add(TimeSpan.Parse(timeSpanString));
                }
                else
                {
                    throw new JsonSerializationException("Invalid TimeSpan format in list");
                }
            }
            return timeSpanList;
        }

        throw new JsonSerializationException("Expected an array of TimeSpan strings");
    }
}
