class Obstacle
{
    public int X;
    public int Y;
    public int Weight;
    public string Type;

    public void RunEachFunc()
    {
        switch (Type)
        {
            case "Kill Player":
                Console.Clear();
                Console.WriteLine($"Game Over.. {Type}");
                Environment.Exit(2);
                break;

            case "Pass Player":
                break;
        }
    }
}