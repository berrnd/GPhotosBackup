namespace GPhotosBackup
{
    public class Arguments
    {
        private Arguments()
        { }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Destination { get; set; }

        public static Arguments Parse(string[] args)
        {
            Arguments parsed = new Arguments();

            foreach (string item in args)
            {
                if (item.Contains("="))
                {
                    string key = item.Split('=')[0].ToLower();
                    string value = item.Split('=')[1];

                    switch (key)
                    {
                        case "username":
                            parsed.Username = value;
                            break;
                        case "password":
                            parsed.Password = value;
                            break;
                        case "destination":
                            parsed.Destination = value;
                            break;
                    }
                }
            }

            return parsed;
        }
    }
}
