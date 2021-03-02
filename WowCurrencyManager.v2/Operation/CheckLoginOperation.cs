using WebBotCore.Operation;

namespace WowCurrencyManager.v2.Operation
{
    public class CheckLoginOperation : OperationBase
    {
        protected override void Action()
        {
            driver.Navigate().GoToUrl("https://www.g2g.com/order/sellOrder?status=5");
        }


    }
}
