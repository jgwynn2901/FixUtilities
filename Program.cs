using System;
using System.Linq;
using DbClassLibrary;
using DbClassLibrary.Opm;
using FileUtilities;
using FileUtilities.Parsers;

namespace FixUtilities
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const string testFile = @"f:\pepsi.csv";

            using (IFileLoader loader = new AsciiTextFileLoader(testFile))
            {
                var parser = new CsvParser();
                var lineCount = 0;
                foreach (var line in loader.Records())
                {
                    parser.Parse(line);
                    if (lineCount++ == 0) continue;
                    if (AlreadyLoaded(parser)) continue;
                    ExecuteInsert(parser);
                    Console.WriteLine(parser.ToString());
                }
                Console.WriteLine($"{lineCount} records processed.");
            }
        }

        private static void ExecuteInsert(IParser parser)
        {
            var sql =
                $"INSERT INTO TRANSMISSION_OUTCOME (CALL_ID, ROUTING_PLAN_ID, STATUS) VALUES ({parser["CALL_ID"].Value},{parser["ROUTING_PLAN_ID"].Value}, 'UNPROCESSED' ";
            GeneralUtility.ExecuteNonQuery(sql,DbBaseClass.SEDP);
        }

        private static bool AlreadyLoaded(IParser parser)
        {
            var results = new TranOutcomeRecordSet
            {
                Instance = DbBaseClass.SEDP,
                Query = string.Format(
                    $"select * from TRANSMISSION_OUTCOME where CALL_ID = {parser["CALL_ID"].Value} and ROUTING_PLAN_ID = {parser["ROUTING_PLAN_ID"].Value}")
            };
            return results.Results().Any();

        }
    }
}
