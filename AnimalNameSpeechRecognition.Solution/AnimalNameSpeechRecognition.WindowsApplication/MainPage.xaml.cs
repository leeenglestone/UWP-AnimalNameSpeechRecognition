using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace AnimalNameSpeechRecognition.WindowsApplication
{
    public sealed partial class MainPage
    {
        SpeechRecognizer _speechRecognizer;
        private CoreDispatcher _dispatcher;

        public MainPage()
        {
            InitializeComponent();
        }

        public async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            bool permissionGained = await AudioCapturePermissions.RequestMicrophonePermission();

            if (permissionGained)
            {
                await InitializeRecognizer(SpeechRecognizer.SystemSpeechLanguage);
                await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async Task InitializeRecognizer(Language recognizerLanguage)
        {
            if (_speechRecognizer != null)
            {
                _speechRecognizer.ContinuousRecognitionSession.Completed -= ContinuousRecognitionSession_Completed;
                _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -= ContinuousRecognitionSession_ResultGenerated;

                _speechRecognizer.Dispose();
                _speechRecognizer = null;
            }

            _speechRecognizer = new SpeechRecognizer(recognizerLanguage);

            var responses = GetAnimalList();
            var listConstraint = new SpeechRecognitionListConstraint(responses, "Animals");
            _speechRecognizer.Constraints.Add(listConstraint);
            await _speechRecognizer.CompileConstraintsAsync();

            _speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            _speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (_speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium || args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await ShowAnimal(args.Result.Text);
                });
            }
        }

        private Task ShowAnimal(string text)
        {
            string cleanedText = text.ToLower().Replace(".", "");
            string[] animals = GetAnimalList();

            if (animals.Contains(cleanedText))
            {
                AnimalImage.Source = new BitmapImage(new Uri($"ms-appx:///Images/Animals/{cleanedText}.jpg"));
                AnimalName.Text = cleanedText;
            }
            else
            {
                AnimalName.Text = cleanedText;
            }

            return Task.CompletedTask;
        }

        private static string[] GetAnimalList()
        {
            return new[] {
                "aardvark",
                "badger",
                "dolphin",
                "duck",
                "fox",
                "guinea pig",
                "hamster",
                "kangaroo",
                "meerkat",
                "mouse",
                "owl",
                "panda",
                "pig",
                "monkey",
                "elephant",
                "rhino",
                "giraffe",
                "penguin",
                "lion",
                "tiger",
                "snake",
                "fish",
                "dog",
                "cat",
                "rabbit",
                "bear",
                "snake",
                "frog",
                "lizard",
                "tortoise",
                "cow",
                "goat",
                "hippo",
                "horse",
                "sheep",
                "zebra",
                "donkey",
                "bird",
                "whale",
            };
        }
    }

    /// <summary>
    /// This was copied entirely from a Microsoft sample
    /// </summary>
    public class AudioCapturePermissions
    {
        // If no recording device is attached, attempting to get access to audio capture devices will throw 
        // a System.Exception object, with this HResult set.
        private static int NoCaptureDevicesHResult = -1072845856;

        /// <summary>
        /// On desktop/tablet systems, users are prompted to give permission to use capture devices on a 
        /// per-app basis. Along with declaring the microphone DeviceCapability in the package manifest,
        /// this method tests the privacy setting for microphone access for this application.
        /// Note that this only checks the Settings->Privacy->Microphone setting, it does not handle
        /// the Cortana/Dictation privacy check, however (Under Settings->Privacy->Speech, Inking and Typing).
        /// 
        /// Developers should ideally perform a check like this every time their app gains focus, in order to 
        /// check if the user has changed the setting while the app was suspended or not in focus.
        /// </summary>
        /// <returns>true if the microphone can be accessed without any permissions problems.</returns>
        public async static Task<bool> RequestMicrophonePermission()
        {
            try
            {
                // Request access to the microphone only, to limit the number of capabilities we need
                // to request in the package manifest.
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                settings.MediaCategory = MediaCategory.Speech;
                MediaCapture capture = new MediaCapture();

                await capture.InitializeAsync(settings);
            }
            catch (TypeLoadException)
            {
                // On SKUs without media player (eg, the N SKUs), we may not have access to the Windows.Media.Capture
                // namespace unless the media player pack is installed. Handle this gracefully.
                var messageDialog = new Windows.UI.Popups.MessageDialog("Media player components are unavailable.");
                await messageDialog.ShowAsync();
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                // The user has turned off access to the microphone. If this occurs, we should show an error, or disable
                // functionality within the app to ensure that further exceptions aren't generated when 
                // recognition is attempted.
                return false;
            }
            catch (Exception exception)
            {
                // This can be replicated by using remote desktop to a system, but not redirecting the microphone input.
                // Can also occur if using the virtual machine console tool to access a VM instead of using remote desktop.
                if (exception.HResult == NoCaptureDevicesHResult)
                {
                    var messageDialog = new Windows.UI.Popups.MessageDialog("No Audio Capture devices are present on this system.");
                    await messageDialog.ShowAsync();
                    return false;
                }
                else
                {
                    throw;
                }
            }
            return true;
        }
    }
}
