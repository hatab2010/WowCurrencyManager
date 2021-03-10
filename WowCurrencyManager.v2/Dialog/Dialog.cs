using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WowCurrencyManager.v2.Data;
using Discord.Rest;
using Discord;
using WowCurrencyManager.v2.Manager;

namespace WowCurrencyManager.v2.Model
{
    public abstract class Dialog
    {
        public ISocketMessageChannel Channel { private set; get; }
        public static List<Dialog> OpenDialogs = new List<Dialog>();
        public List<IMessage> DialogMessages = new List<IMessage>();

        public Dialog(ISocketMessageChannel context)
        {
            Channel = context;
        }
    }

    public class RegistrationFarmRoomDialog : Dialog
    {
        public Channel Result { private set; get; } = new Channel();
        IDialogStep CurrentStep;
        public List<IDialogStep> Steps { private set; get; }
        

        public RegistrationFarmRoomDialog(ISocketMessageChannel context) : base(context)
        {
            Result = new Channel()
            {
                Id = Channel.Id,
                ChannelRole = ChannelRole.Farm
            };

            Steps = new List<IDialogStep>()
            {
                new ServerNameStep(),
                new FractionStep(),
                new WordPartStep()
            };

            CurrentStep = Steps[0];
            SendAnswer().Wait();
        }

        private bool IsHaveNextStep()
        {
            var currentIndex = Steps.IndexOf(CurrentStep);
            var nextStepIndex = currentIndex + 1;

            if (nextStepIndex < Steps.Count)
            {                
                return true;
            }
            else
            {
                return false;
            }
        }

        private async Task ClearChat()
        {
            foreach (var msg in DialogMessages)
            {
                try
                {
                    await msg.DeleteAsync();
                }
                catch (Exception)
                {

                }

            }

            DialogMessages.Clear();
        }

        internal async Task SendAnswer(SocketUserMessage message = null)
        {
            if (message != null)
                DialogMessages.Add(message);

            await ClearChat();

            if (message == null)
            {
                var rest = await Channel.SendMessageAsync(CurrentStep.Description);
                DialogMessages.Add(rest);
                return;
            }           

            try
            {
                CurrentStep.TakeValue(message, Result);
            }
            catch (DialogValueException ex)
            {
                var rest = await Channel.SendMessageAsync($"{ex.Message}");
                DialogMessages.Add(rest);
                return;
            }
            

            if (IsHaveNextStep())
            {
                CurrentStep = Steps[Steps.IndexOf(CurrentStep) + 1];
                var rest = await Channel.SendMessageAsync(CurrentStep.Description);
                DialogMessages.Add(rest);
            }
            else
            {
                Finish();
            }            
        }

        private void Finish()
        {
            using (var db = new MobileContext())
            {
                var manager = new DataManager(db);
                var contextGuild = (Channel as IGuildChannel).Guild;
                var channel = db.Channels.FirstOrDefault(_ => _.Id == Result.Id);
                var guild = db.Guilds.FirstOrDefault(_ => _.Id == contextGuild.Id);

                Result.GuildId = contextGuild.Id;

                if (guild == null)
                {
                    guild = new Guild()
                    {
                        Name = contextGuild.Name,
                        Id = contextGuild.Id
                    };
                }

                if (channel == null)
                {
                    channel = Result;
                }

                manager.Save(guild);
                manager.Save(channel);
            }

            OpenDialogs.Remove(this);
        }
    }

    public class ServerNameStep : IDialogStep
    {
        public string Description { get; } = "Введите имя сервера";

        public void TakeValue(SocketUserMessage message, Channel channel)
        {
            channel.ServerName = message.Content;
        }
    }
    public class WordPartStep : IDialogStep
    {
        public string Description { get; } = "Введите часть света [us/eu]";

        public void TakeValue(SocketUserMessage message, Channel channel)
        {
            WorldPart result;
            var isExist = Enum.TryParse<WorldPart>(message.Content, true, out result);

            if (isExist)
            {
                channel.WorldPart = result;
            }
            else
            {
                throw new DialogValueException("Некорректно выбрана часть света");
            }
        }

    }
    public class FractionStep : StepBase, IDialogStep
    {
        public string Description { get; } = "Введите фракцию сервера";

        public void TakeValue(SocketUserMessage message, Channel channel)
        {
            Fraction result;
            var isExist = Enum.TryParse<Fraction>(message.Content,true, out result);

            if (isExist)
            {
                channel.Fraction = result;
            }
            else
            {                
                throw new DialogValueException("Выбранной фракции не существует");
            }
        }
    }
    public class DialogValueException : Exception
    {
        public DialogValueException(string msg) : base(msg)
        {

        }
    }

    public abstract class StepBase
    {
    }

    public interface IDialogStep
    {
        string Description { get; }
        void TakeValue(SocketUserMessage message, Channel channel);
    }
}
