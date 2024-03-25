using Newtonsoft.Json;
using System.Text.RegularExpressions;
using TrialProjectRandomUsers.Models;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace TrialProjectRandomUsers
{
    public partial class StatisticsHelper
    {
        public StatisticsHelper() { }

        [GeneratedRegex("^[A-M]")]
        private static partial Regex StartWithAToMRegex();
        public double getPercentageFemale(Person root)
        {
            var TotalCount = root.Results.Count;
            var FemaleCount = (from Result in root.Results
                               where Result.Gender == "female"
                               select Result.Gender).Count();
            var percentageFemale = Math.Round(FemaleCount * 100.0 / TotalCount, 2);
            return percentageFemale;
        }
        public double getPercentageMale(Person root)
        {
            var TotalCount = root.Results.Count;
            var MaleCount = (from Result in root.Results
                             where Result.Gender == "male"
                             select Result.Gender).Count();
            var percentageMale = Math.Round(MaleCount * 100.0 / TotalCount, 2);
            return percentageMale;
        }

        public double getPercentageFirstNameAToM(Person root)
        {
            var TotalCount = root.Results.Count;
            var FirstNameAMCount = (from Result in root.Results
                                    where (StartWithAToMRegex().Match(Result.Name.First).Success)
                                    select Result.Gender).Count();

            var percentageFirstNameAToM = FirstNameAMCount * 100.0 / TotalCount;
            return percentageFirstNameAToM;
        }

        public double getPercentageLastNameAToM(Person root)
        {
            var TotalCount = root.Results.Count;
            var LastNameAMCount = (from Result in root.Results
                                   where (StartWithAToMRegex().Match(Result.Name.Last).Success)
                                   select Result.Gender).Count();

            var percentageLastNameAToM = Math.Round(LastNameAMCount * 100.0 / TotalCount, 2);
            return percentageLastNameAToM;
        }

        public dynamic getResultPeopleInEachTopTenState(Person root)
        {
            List<Result> peopleData = root.Results; // Replace with your data source

            var results = (from p in peopleData
                           group p.Gender by p.Location.State into g
                           select new { state = g.Key, stateTotalPopulation = g.Count() })
                            .OrderByDescending(state => state.stateTotalPopulation)
                                               .Take(10);
            return results;
        }

        public dynamic getFemaleInEachTopTenState(Person root)
        {
            List<Result> peopleData = root.Results;
            var results= (from p in peopleData
                            group p by p.Location.State into g
                            select new { state = g.Key, stateTotalPopulation = g.Count(), FemaleCount = g.Where(s => s.Gender == "female").Count() })
                                       .OrderByDescending(state => state.stateTotalPopulation)
                                                          .Take(10);
            return results;
        }
        public dynamic getMaleInEachTopTenState(Person root)
        {
            List<Result> peopleData = root.Results;
            var results = (from p in peopleData
             group p by p.Location.State into g
             select new { state = g.Key, stateTotalPopulation = g.Count(), MaleCount = g.Where(s => s.Gender == "male").Count() })
                                                 .OrderByDescending(state => state.stateTotalPopulation)
                                                                    .Take(10);
            return results;
        }

        public dynamic getAgeGroupPopulation(Person root)
        {
            int[] ageGroupThresholds = { 0, 21, 41, 61, 81, 100 }; // Thresholds (0-20, 21-40, 41-60, 61-80, 81-100, 100+)
            List<Result> peopleData = root.Results;
            var peopleByAgeGroup = peopleData.GroupBy(person =>
            {
                int age = person.Dob.Age;
                for (int i = 0; i < ageGroupThresholds.Length - 1; i++)
                {
                    if (age >= ageGroupThresholds[i] && age < ageGroupThresholds[i + 1])
                    {
                        return i; // Age group index
                    }
                }
                return ageGroupThresholds.Length - 1; // Default to last group for outliers
            })
            .Select(group => new
            {
                AgeGroup = $"{(group.Key == 0 ? "0-" : ageGroupThresholds[group.Key])}{(ageGroupThresholds[group.Key + 1] == 100 ? "+" : "-" + (ageGroupThresholds[group.Key + 1] - 1).ToString())}",
                Count = group.Count()
            });

            return peopleByAgeGroup;
        }

        public string GetStatistics(Person root, string fileType)
        {
            var TotalCount = root.Results.Count;
            string txtStatistics = "";
            //1. Percentage of gender in each category
            var percentageFemale = getPercentageFemale(root);
            var percentageMale = getPercentageMale(root);
            txtStatistics += "1. Percentage of gender in each category" + "\r\n";
            txtStatistics += "Percentage of Female: " + percentageFemale + "%.\r\n";
            txtStatistics += "Percentage of Male: " + percentageMale + "%.\r\n";
            //2. Percentage of first names that start with A-M versus N-Z
            var percentageFirstNameAToM = getPercentageFirstNameAToM(root);
            txtStatistics += "2. Percentage of first names that start with A-M versus N-Z" + "\r\n";
            txtStatistics += "Percentage of first names that start with A-M:  " + percentageFirstNameAToM + "%.\r\n";
            //3. Percentage of last names that start with A-M versus N-Z
            txtStatistics += "3. Percentage of last names that start with A-M versus N-Z" + "\r\n";
            var percentageLastNameAToM = getPercentageLastNameAToM(root);
            txtStatistics += "Percentage of last names that start with A-M: " + percentageLastNameAToM + "%.\r\n";
            //4. Percentage of people in each state, up to the top 10 most populous states
            txtStatistics += "4. Percentage of people in each state, up to the top 10 most populous states" + "\r\n";         
            var results = getResultPeopleInEachTopTenState(root);
            foreach (var result in results)
            {
                txtStatistics += "Percentage of people in " + result.state + ": " + Math.Round(result.stateTotalPopulation * 100.0 / TotalCount, 2) + "%." + "\r\n";
            }
            //5. Percentage of females in each state, up to the top 10 most populous states
            txtStatistics += "5. Percentage of females in each state, up to the top 10 most populous states" + "\r\n";
            var results5 = getFemaleInEachTopTenState(root);
            foreach (var result in results5)
            {
                txtStatistics += "Percentage of female in " + result.state + ": " + Math.Round(result.FemaleCount * 100.0 / result.stateTotalPopulation, 2) + "%." + "\r\n";
            }          
            //6. Percentage of males in each state, up to the top 10 most populous states
            txtStatistics += "6. Percentage of males in each state, up to the top 10 most populous states" + "\r\n";
            var results6 = getMaleInEachTopTenState(root);
            foreach (var result in results6)
            {
                txtStatistics += "Percentage of male in " + result.state + ": " + Math.Round(result.MaleCount * 100.0 / result.stateTotalPopulation, 2) + "%." + "\r\n";
            }
            //7. Percentage of people in the following age ranges: 0-20, 21-40, 41-60, 61-80, 81-100, 100+
            txtStatistics += "7. Percentage of people in the following age ranges: 0-20, 21-40, 41-60, 61-80, 81-100, 100+" + "\r\n";
            int[] ageGroupThresholds = { 0, 21, 41, 61, 81, 100 }; // Thresholds (0-20, 21-40, 41-60, 61-80, 81-100, 100+)
            var peopleByAgeGroup = getAgeGroupPopulation(root);
            foreach (var result in peopleByAgeGroup)
            {
                txtStatistics += "Percentage of persons in age group " + result.AgeGroup + ": " + Math.Round(result.Count * 100.0 / TotalCount, 2) + "%." + "\r\n";
            }
            var filePath = Path.GetTempPath();
            var filePathTxt = filePath + @"\RandomStatistics.txt";
            if (fileType == "txt")
            {
                File.WriteAllText(filePathTxt, txtStatistics);
                return txtStatistics;
            }
            // Write the data to the JSON file
            var PercentageOfPeopleInTop10States = new JObject();
            var PercentageOfFemaleInTop10States = new JObject();
            var PercentageOfMaleInTop10States = new JObject();
            var PercentageOfPersonsInEachAgeGroup = new JObject();

            dynamic statistics = new JObject();
            statistics.PercentageOfFemale = percentageFemale.ToString() + "%";
            statistics.PercentageOfMale = percentageMale.ToString() + "%";
            statistics.PercentageOfFirstNameAToM = percentageFirstNameAToM.ToString() + "%";
            statistics.PercentageOfLastNameAToM = percentageLastNameAToM.ToString() + "%";
            statistics.PercentageOfPeopleInTop10States = PercentageOfPeopleInTop10States;
            statistics.PercentageOfFemaleInTop10States = PercentageOfFemaleInTop10States;
            statistics.PercentageOfMaleInTop10States = PercentageOfMaleInTop10States;
            statistics.PercentageOfPersonsInEachAgeGroup = PercentageOfPersonsInEachAgeGroup;

            foreach (var result in results)
            {
                JProperty subLayer1 = new (
                    // Add properties and values for the sub-layer here
                    result.state,Math.Round(result.stateTotalPopulation * 100.0 / TotalCount, 2) + "%"
                );
                PercentageOfPeopleInTop10States.Add(subLayer1);
            }
            foreach (var result in results5)
            {
                JProperty subLayer1 = new (
                    // Add properties and values for the sub-layer here
                    result.state,
                    Math.Round(result.FemaleCount * 100.0 / result.stateTotalPopulation, 2) + "%"
                );
                PercentageOfFemaleInTop10States.Add(subLayer1);
            }
            foreach (var result in results6)
            {
                JProperty subLayer1 = new (
                    // Add properties and values for the sub-layer here
                    result.state,
                    Math.Round(result.MaleCount * 100.0 / result.stateTotalPopulation, 2) + "%"
                );
                PercentageOfMaleInTop10States.Add(subLayer1);
            }
            foreach (var result in peopleByAgeGroup)
            {
                JProperty subLayer1 = new (
                   // Add properties and values for the sub-layer here
                   result.AgeGroup,
                    Math.Round(result.Count * 100.0 / TotalCount, 2) + "%"
                );                
                PercentageOfPersonsInEachAgeGroup.Add(subLayer1);
            }
           
            // Serialize the JObject to a JSON string
            string jsonString = JsonConvert.SerializeObject(statistics, Newtonsoft.Json.Formatting.Indented);

            // Write the JSON string to the file
            if (fileType == "json")
            {               
                return jsonString;
            }
            
            var doc = JsonConvert.DeserializeXmlNode(jsonString, "root");
            // Serialize the JObject to a JSON string
            string jString = JsonConvert.SerializeObject(doc);
            string jnString = fixEncoding(jString);
            
            // Parse the JSON string into an XDocument
            XDocument xDocument = JsonConvert.DeserializeObject<XDocument>(jnString);          
            // Access the root XElement from the XDocument
            XElement rootElement = xDocument.Root;
            // Save the XML to a file    
            if (fileType.ToLower() == "xml")
            {   
                return xDocument.ToString();
            }
            return "File format is not supported.";
        }
        public string fixEncoding(string jString)
        {
            return jString.Replace("-", "To").Replace("21To", "From21To").Replace(" ", "").Replace("_", "")
                .Replace("41To", "From41To").Replace("61To", "From61To").Replace("81To", "From81To")
                .Replace("100+", "Age100Plus");
        }

    }
}
