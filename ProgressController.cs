    public class ProgressController : Controller
    {
        private readonly IMemoryCache _cache;
        public ProgressController(IMemoryCache cache)
        {
            _cache = cache;
        }
        
        // 啟動任務
        [HttpPost]
        public IActionResult StartJob()
        {
            var jobId = Guid.NewGuid().ToString();
            _cache.Set(jobId, new ProgressInfo
            { 
                Percent = 0,
                Message = "開始處理...",
                StartTime = DateTime.Now
            });

            // 背景執行模擬多段任務
            Task.Run(async () =>
            {
                await RunStep(jobId, 25, "第一段完成");
                await RunStep(jobId, 50, "第二段完成");
                await RunStep(jobId, 75, "第三段完成");
                await RunStep(jobId, 100, "全部完成", true);
            });

            return Json(new { JobId = jobId });
        }    

        // 查詢進度
        [HttpGet]
        public IActionResult GetProgress(string jobId)
        {
            if (_cache.TryGetValue(jobId, out ProgressInfo? info))
            {
                return Json(info);
            }
            return Json(new ProgressInfo { Percent = 0, Message = "尚未開始" });
        }        

        // 模擬每段任務
        private async Task RunStep(string jobId, int percent, string message, bool isLast = false)
        {
            await Task.Delay(2000); // 模擬耗時
            if (_cache.TryGetValue(jobId, out ProgressInfo? info))
            {
                info.Percent = percent;
                info.Message = message;
                if (isLast) info.EndTime = DateTime.Now;
                _cache.Set(jobId, info);
            }
        }
    }   

    public class ProgressInfo
    {
        public int Percent { get; set; }
        public string? Message { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
    }
