﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using Windows.Data.Pdf;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
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
        private CoreCursor _tempCursor;



        public MainPage()
        {
            this.InitializeComponent();
        }


        public async void OpenLocal()
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.FileTypeFilter.Add("*");
            picker.FileTypeFilter.Add(".pdf");

            StorageFile file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                // Application now has read/write access to the picked file
                PdfDocument doc = await PdfDocument.LoadFromFileAsync(file);

                // Loads PDF file into GUI
                Load(doc);
            }

        } // end OpenLocal()


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

            // Gets the PDF file URI
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                Url = inputTextBox.Text;
            }

            if ( Url != null && !Url.Equals("") )
            {
                // Verifies 'Url'
                if ( ! Url.Contains("://") )
                {
                    Url = "http://" + Url;
                }

                // Change the cursor to wait cursor
                _tempCursor = Window.Current.CoreWindow.PointerCursor;
                Window.Current.CoreWindow.PointerCursor = new Windows.UI.Core.CoreCursor(Windows.UI.Core.CoreCursorType.Wait, 1);

                try
                {
                    // Retrieves the remote PDF file as a stream of bytes
                    HttpClient client = new HttpClient();
                    var stream = await client.GetStreamAsync(Url);

                    // Puts the stream into memory
                    var memStream = new MemoryStream();
                    await stream.CopyToAsync(memStream);
                    memStream.Position = 0;

                    // Creates PDF document from the downloaded stream and loads into GUI
                    PdfDocument doc = await PdfDocument.LoadFromStreamAsync(memStream.AsRandomAccessStream());
                    Load(doc);
                }
                catch (Exception exception)
                {
                    // Display the exception in a 'MessageDialog'
                    MessageDialog showDialog = new MessageDialog(exception.Message);
                    showDialog.Commands.Add(new UICommand("Ok"));
                    await showDialog.ShowAsync();
                }
                finally
                {
                    // Restores the cursor to normal
                    Window.Current.CoreWindow.PointerCursor = _tempCursor;
                }

            }

        } // end OpenRemote()


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

        } // end Load(PdfDocument pdfDoc)


        public ObservableCollection<BitmapImage> PdfPages
        {
            get;
            set;
        } = new ObservableCollection<BitmapImage>();

    } // end class MainPage

} // end namespace PDFViewer
