using Analogy.Interfaces;
using Analogy.Interfaces.Factories;
using Analogy.LogViewer.Philips.CustomActions;
using Analogy.LogViewer.Philips.DataSources;
using System;
using System.Collections.Generic;

namespace Analogy.LogViewer.Philips.Factories
{
    public class ICAPFactory : IAnalogyFactory
    {
        internal static Guid Id = new Guid("F0FD2BC8-5DA5-4A7D-8E09-4E4C411EDA0C");
        public Guid FactoryId { get; } = Id;
        public string Title { get; } = "Philips ICAP BU Logs";
        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; } = new List<IAnalogyChangeLog>(0);
        public IEnumerable<string> Contributors { get; } = new List<string> { "Lior Banai" };
        public string About { get; } = "Created by Lior Banai";
    }

    public class ICAPDataSourcesFactory : IAnalogyDataProvidersFactory
    {
        public Guid FactoryId { get; } = ICAPFactory.Id;
        public string Title { get; } = "Philips ICAP BU Data Sources";
        public IEnumerable<IAnalogyDataProvider> DataProviders { get; } = new List<IAnalogyDataProvider> { new OfflineICAPLog() };
    }

    public class ICAPCustomActionFactory : IAnalogyCustomActionsFactory
    {
        public Guid FactoryId { get; } = ICAPFactory.Id;
        public string Title { get; } = "Philips ICAP BU Tools";

        IEnumerable<IAnalogyCustomAction> IAnalogyCustomActionsFactory.Actions { get; } =
            new List<IAnalogyCustomAction> { new FixCorruptedFilelAction(), new LogConfiguratorAction() };

    }

    //public class ICAPUserControlFactory : IAnalogyUserControlFactory
    //{
    //    public string Title { get; } = "Philips ICAP BU User Controls";
    //    public IEnumerable<IAnalogyUserControl> Items { get; }
    //    public ICAPUserControlFactory()
    //    {
    //        Items = new List<IAnalogyUserControl>(0);
    //    }

    //}
}
