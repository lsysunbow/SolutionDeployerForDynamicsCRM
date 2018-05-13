using System; 
namespace DeployTool
{
    public class Program
    {  
        public static void Main(string[] args)
        { 
            try
            {
                Console.WriteLine("Do you Confirm to deploy?(press y to deploy) ");
                string key = Console.ReadLine();
                if (key == "y")
                {  
                    Deploy deploy = new Deploy();
                    deploy.RunAsync().Wait();
                    //deploy.GenerateSettings();
                } 
                Console.Read();

            }
            catch (Exception ex)
            {
                Console.WriteLine("error occured! {0}", ex.Message);
                Console.ReadKey();
            }
        }


    }
}
