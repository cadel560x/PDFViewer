using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PDFViewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        public async void OpenLocal()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add(".pdf");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                // Application now has read/write access to the picked file
                PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);
                Load(doc);
            }
             
        }

        public async void OpenRemote()
        {
            string Url = null;
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;

            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = "Remote PDF file URL";
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Url = inputTextBox.Text;
            }

            if ( Url != null && !Url.Equals("") )
            {
                HttpClient client = new HttpClient();
                var stream = await client.GetStreamAsync(Url);
                var memStream = new MemoryStream();
                await stream.CopyToAsync(memStream);
                memStream.Position = 0;
                PdfDocument doc = await PdfDocument.LoadFromStreamAsync(memStream.AsRandomAccessStream());

                Load(doc);
            }
        }

        async void Load(PdfDocument pdfDoc)
        {
            PdfPages.Clear();

            for (uint i = 0; i < pdfDoc.PageCount; i++)
            {
                BitmapImage image = new BitmapImage();

                var page = pdfDoc.GetPage(i);

                using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
                {
                    await page.RenderToStreamAsync(stream);
                    await image.SetSourceAsync(stream);
                }

                PdfPages.Add(image);
            }
        }

        public ObservableCollection<BitmapImage> PdfPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();
    }
}
