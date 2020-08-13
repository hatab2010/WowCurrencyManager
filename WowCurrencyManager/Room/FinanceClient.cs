﻿using Discord;
using System;

namespace WowCurrencyManager.Room
{
    [Serializable]
    public class FinanceClient
    {
        static public Action Changed;
        private int balance;
        private int uSDBalance;

        public int Balance
        {
            get => balance; private
            set
            {
                balance = value;
                Changed?.Invoke();
            }
        }
        public ulong Id { private set; get; }
        public int USDBalance
        {
            get => uSDBalance;
            private set
            {
                uSDBalance = value;
                Changed?.Invoke();
            }
        }
        public string Name { private set; get; }
        public FinanceClient(ulong id, string name)
        {
            Id = id;
            Name = name;
        }

        internal void AddBalance(int value)
        {
            Balance += value;
        }

        public void AddUSDBalance(int value)
        {
            USDBalance += value;
        }

        public void RefrashBalance()
        {
            Balance = 0;
            USDBalance = 0;
        }

        internal void RemoveBalance(int value)
        {
            Balance -= value;
        }

        public Embed SellEmbedBuild(int value)
        {
            var builder = new EmbedBuilder();
            var footer = new EmbedFooterBuilder();
            footer.Text = $"общий дебет: {Balance}";

            builder.WithDescription($"{Name} продал: {value}");
            builder.Color = Color.Blue;
            builder.WithFooter(footer);

            return builder.Build();
        }


        public Embed BalanceBuild()
        {
            var builder = new EmbedBuilder();
            builder.WithDescription($"{Name} общий дебет: {Balance}");
            builder.Color = Color.Blue;

            return builder.Build();
        }

        public Embed BalanceUSDBuild()
        {
            var builder = new EmbedBuilder();
            builder.WithDescription($"{Name} итоговый дебет: {USDBalance}$");
            builder.Color = Color.DarkBlue;

            return builder.Build();
        }

        public Embed RemoveEmbedBuilder(int value)
        {
            var builder = new EmbedBuilder();
            var footer = new EmbedFooterBuilder();
            footer.Text = $"общий дебет: {Balance}";

            builder.WithDescription($"{Name} отменил: {value}");
            builder.Color = Color.DarkBlue;
            builder.WithFooter(footer);

            return builder.Build();
        }

        public Embed PayEmbedBuild()
        {
            var builder = new EmbedBuilder();
            builder.WithDescription($"{Name} итоговый дебет: {Balance}");
            return builder.Build();
        }
    }
}