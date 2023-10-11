namespace AutoGram
{
    class Work
    {
        public static void Do(object w)
        {
            Worker worker = w as Worker;

            Task.Main.Do(worker);

            worker.IsWork = false;
        }
    }
}
