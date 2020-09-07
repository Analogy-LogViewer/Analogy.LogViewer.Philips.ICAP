using System;
using System.Collections.Generic;
using System.Drawing;
using Analogy.Interfaces;
using Analogy.Interfaces.Factories;
using Analogy.LogViewer.Philips.ICAP.CustomActions;
using Analogy.LogViewer.Philips.ICAP.DataSources;
using Analogy.LogViewer.Philips.ICAP.Properties;

namespace Analogy.LogViewer.Philips.ICAP.Factories
{
    public class ICAPFactory : IAnalogyFactory
    {
        internal static Guid Id = new Guid("F0FD2BC8-5DA5-4A7D-8E09-4E4C411EDA0C");
        public Guid FactoryId { get; set; } = Id;
        public string Title { get; set; } = "Philips ICAP BU Logs";
        public IEnumerable<IAnalogyChangeLog> ChangeLog { get; set; } = new List<IAnalogyChangeLog>(0);
        public Image LargeImage { get; set; } = Resources.philips_image_32x32;
        public Image SmallImage { get; set; } = Resources.philips_image_16x16;
        public IEnumerable<string> Contributors { get; set; } = new List<string> { "Lior Banai" };
        public string About { get; set; } = "Created by Lior Banai";
    }

    public class ICAPDataSourcesFactory : IAnalogyDataProvidersFactory
    {
        public Guid FactoryId { get; set; } = ICAPFactory.Id;
        public string Title { get; set; } = "Philips ICAP BU Data Sources";
        public IEnumerable<IAnalogyDataProvider> DataProviders { get; } = new List<IAnalogyDataProvider> { new OfflineICAPLog() };
    }

    public class ICAPCustomActionFactory : IAnalogyCustomActionsFactory
    {
        public Guid FactoryId { get; set; } = ICAPFactory.Id;
        public string Title { get; set; } = "Philips ICAP BU Tools";

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
