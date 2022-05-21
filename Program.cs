namespace SdamgiaParser;

internal class Program {
    const string Subject = "math-ege";
    const string BaseUrl = $"https://{Subject}.sdamgia.ru/get_solution?id=";
    const int Threads = 100;

    static async Task Main(string[ ] args) {

        // Будет создана только если не существует
        Directory.CreateDirectory("Photos");

        // "Photos\158796274.WNhgfhg.jpg"
        var lastidt = Directory.GetFiles("Photos").FirstOrDefault( )[7..].Split('.')[0];
        var lastid = int.Parse(lastidt) - 1;
        object _lock = new( );

        CancellationTokenSource cancelTokenSource = new( );
        CancellationToken token = cancelTokenSource.Token;

        int GetNextId( ) {
            lock (_lock) {
                if (lastid <= 1) {
                    cancelTokenSource.Cancel( );
                }
                lastid -= 1;
                return lastid;
            }
        }

        ThreadId = 1;

        Task[ ] threads = new Task[Threads];
        for (int x = 0; x < Threads; x++) {
            threads[x] = Task.Run(async ( ) => {
                int threadid = ThreadId;
                Console.WriteLine("Thread " + threadid + " started");
                HttpClient client = new( );
                while (!token.IsCancellationRequested) {
                    int i = GetNextId( );
                    Console.Write($"\n[{threadid}] Get {i}");
                    var a = (await client.GetAsync(BaseUrl + i)).Content;

                    // Если файла не существует
                    if (a.Headers.ContentDisposition is null) {
                        continue;
                    }
                    string filename = a.Headers.ContentDisposition.FileName[1..^1];
                    Console.Write(" >> " + filename);

                    using var fs = new FileStream(Path.Combine("Photos", $"{i}.{filename}"), FileMode.CreateNew);
                    await a.CopyToAsync(fs);
                }
            });
        }
        Task.WaitAll(threads);
    }
    public static int ThreadId {
        get {
            return _threadId++;
        }
        set {
            _threadId = value;
        }
    }
    private static int _threadId { get; set; }
}
