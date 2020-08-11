using Discord;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Model
{
    public class G2gOrder
    {
        public string OrderId;
        public string Server;
        public string Fraction;
        public int Amount;
        public string Buyer;
        public RoomClient Performer;

        public void SetPerformer(RoomClient performer)
        {
            Performer = performer;
        }

        public Embed GetOrderEmbed()
        {
            var builder = new EmbedBuilder();             

            builder.WithTitle(Server);
            builder.AddField("OrderId", OrderId, false);
            builder.AddField("Gold", Amount, false);
            builder.AddField("Buyer", Buyer, false);
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/742113122592227450/742723748485922826/hideaway-logo-final-flat-max.jpg");

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
