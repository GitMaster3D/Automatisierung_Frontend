using Api_Connection;

namespace Automatisierung_Frontend
{
    public class MessageConfigurator
    {
        public static async void UpdateWateringData(int targetPump, WateringData wateringData)
        {
            wateringData.TargetPump = targetPump;

            await API_Connection.Post("/setprogram", wateringData);
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
            set => TargetMoisturePercentage = value < 0 ? 0 : value > 100 ? 100 : value;
        }
        private float TargetMoisturePercentage;

	    public float TargetDailyWater_Liters { get; set; }
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
