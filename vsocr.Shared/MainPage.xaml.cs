using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using WindowsPreview.Media.Ocr;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace vsocr
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            btnRecognize.IsEnabled = false;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private readonly OcrEngine _engine = new OcrEngine(OcrLanguage.English);

        private async void btnLoad_Click(object sender, RoutedEventArgs e)
        {
            await LoadImageAsync();
        }

        private async Task LoadImageAsync()
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".jpg", ".jpeg", ".png" }
            };

            var file = await picker.PickSingleFileAsync();
            if (file != null)
                await LoadImageAsync(file);
        }

        private async Task LoadImageAsync(StorageFile file)
        {
            var imageProperties = await file.Properties.GetImagePropertiesAsync();

            using (var imageStream = await file.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new WriteableBitmap((int)imageProperties.Width, (int)imageProperties.Height);
                bitmap.SetSource(imageStream);
                previewImage.Source = bitmap;
            }

            ClearResults();
        }

        private void ClearResults()
        {
            // Retrieve initial state.
            previewImage.RenderTransform = null;

            btnRecognize.IsEnabled = true;

            recognizedTextTxtBlock.Text = "";
        }

        private async void btnExtract_Click(object sender, RoutedEventArgs e)
        {
            await RecognizeText();
        }

        private async Task RecognizeText()
        {
            var bitmap = (WriteableBitmap)previewImage.Source;

                var result = await _engine.RecognizeAsync((uint)bitmap.PixelHeight, (uint)bitmap.PixelWidth,
                    bitmap.PixelBuffer.ToArray());

            if (result.Lines != null)
            {
                var recognizedText = "";
                foreach (var line in result.Lines)
                {
                    foreach (var word in line.Words)
                    {
                        recognizedText += word.Text + " ";
                    }
                    recognizedText += Environment.NewLine;
                }

                recognizedTextTxtBlock.Text = recognizedText;
            }
        }
    }
}
