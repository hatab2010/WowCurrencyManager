using Discord;
using WowCurrencyManager.Room;

namespace WowCurrencyManager.Model
{
    public class G2gOrder
    {
        public FarmRoom RoomSender;
        public bool IsCansel = false;
        public ulong OrderMessageId;
        public string OrderId;
        public string Server;
        public string Fraction;
        public int Amount;
        public string Buyer;
        public string OrderPage;
        public RoomClient Performer;

        public void SetPerformer(RoomClient performer)
        {
            Performer = performer;
        }

        public Embed GetOrderEmbed()
        {
            var builder = new EmbedBuilder();

            builder.WithTitle($"{Server}");
            builder.AddField("Order", OrderId, false);
            builder.AddField("Gold", Amount, false);
            builder.AddField("Buyer", Buyer, false);
            builder.WithThumbnailUrl("https://cdn.discordapp.com/attachments/739498423958372362/757573166347321405/Logo.jpg");

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
