using WebBotCore.Interfase;
using WebBotCore.Model;

namespace WowCurrencyManager.v2.Model
{
    public enum WorkerRole
    {
        Default, Watch, Edit, Video
    }

    public class G2gWorker : WorkerBase
    {
        public WorkerRole Role;

        public G2gWorker(IChromeProfile profile) : base(profile)
        {
        }
    }
}
