using Discord;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Model
{
    public class G2gOrder
    {
        public int OrderId;
        public string server;
        public int Gold;
        public string Buyer;
        public RoomClient Performer;

        public void SetPerformer(RoomClient performer)
        {
            Performer = performer;
        }

        public Embed GetOrderEmbed()
        {
            var builder = new EmbedBuilder();             

            builder.WithTitle(server);
            builder.AddField("OrderId", OrderId, true);
            builder.AddField("Gold", Gold, true);
            builder.AddField("Buyer", Buyer, true);            

            if (Performer != null)
            {
                var footer = new EmbedFooterBuilder();
                footer.WithIconUrl(Performer.AvatarUrl)
                .WithText(Performer.Name);
                builder.AddField("___", "Order delivery by:");
                builder.WithFooter(footer);
                builder.Color = Color.Green;
            }
            else
            {
                builder.Color = Color.Blue;
            }            

            return builder.Build();
        }
    }
}
