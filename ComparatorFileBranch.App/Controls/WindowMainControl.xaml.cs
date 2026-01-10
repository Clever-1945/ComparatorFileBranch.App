using ComparatorFileBranch.App.Definitions;
using Microsoft.Web.WebView2.Core;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;

namespace ComparatorFileBranch.App.Controls
{
    /// <summary>
    /// Логика взаимодействия для WindowMainControl.xaml
    /// </summary>
    public partial class WindowMainControl : UserControl
    {
        private GitHelper gitHelper;

        private string originalContent;
        private string localContent;
        public string LocalFile { set; get; }

        private TaskCompletionSource<bool> taskInitMonacoEditor = new TaskCompletionSource<bool>();

        private Dictionary<string, string> dictionaryLanguage = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        public WindowMainControl()
        {
            InitializeComponent();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                LocalFile = args[1];
                if (File.Exists(LocalFile))
                {
                    FillLanguage();
                    NameLeftTools.SetTextInfo("Инициализирую браузер...");

                    webView.EnsureCoreWebView2Async();
                    DataContext = this;

                    NameLeftTools.OnClickSave = SaveContent;
                    NameLeftTools.OnSelectBranch = OnChangeBranch;
                    webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
                    this.Loaded += OnLoaded;
                }
                else
                {
                    NameLeftTools.SetTextInfo($"Файла {LocalFile} не существует");
                }
            }
            else
            {
                NameLeftTools.SetTextInfo("Приложение запущено без аргументов...");
            }
        }

        private void FillLanguage()
        {
            dictionaryLanguage[".cs"] = "csharp";
            dictionaryLanguage[".js"] = "javascript";
            dictionaryLanguage[".ts"] = "typescript";
            dictionaryLanguage[".css"] = "css";
            dictionaryLanguage[".html"] = "html";
            dictionaryLanguage[".xaml"] = "xml";
            dictionaryLanguage[".json"] = "json";
            dictionaryLanguage[".scss"] = "scss";
            dictionaryLanguage[".xml"] = "xml";
            dictionaryLanguage[".sql"] = "sql";
        }

        private void ReadContent(string branch)
        {
            originalContent = gitHelper.GetContentFile(branch);
            localContent = File.ReadAllText(LocalFile);
        }

        private void ShowContent()
        {
            var originalContent = JsonSerializer.Serialize(this.originalContent);
            var localContent = JsonSerializer.Serialize(this.localContent);

            var language = GetLanguageByLocalFile();
            var script = $"assistant.setModel({originalContent}, {localContent}, '{language}')";
            webView.ExecuteScriptAsync(script);
        }

        private string GetLanguageByLocalFile()
        {
            return dictionaryLanguage.GetValueOrDefault(Path.GetExtension(LocalFile)) ?? "plaintext";
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            gitHelper = new GitHelper(LocalFile);

            Assistant.StartAsync(() =>
            {
                NameLeftTools.SetTextInfo("Получаю ветки...");
                var listBranch = gitHelper.GetListBranch().OrderBy(x => x.IsRemote).ThenBy(x => x.IsCurrent).ThenBy(x => x.Name).ToArray();
                NameLeftTools.SetTextInfo("Инициализирую MonacoEditor...");
                var result = taskInitMonacoEditor.Task.Result;
                NameLeftTools.SetListBranch(listBranch);
            });
        }

        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {            
            NameLeftTools.SetTextInfo("Инициализирую MonacoEditor...");
            string folderPath = Path.Combine(Assistant.ApplicationPath, "wwwroot");
#if DEBUG
            string monacoEditorScriptPath = @"d:\projects\js\monaco-editor-0.55";
#else
            string monacoEditorScriptPath = Path.Combine(Assistant.ApplicationPath, "wwwroot", "libs", "monaco-editor");
            if (!Directory.Exists(monacoEditorScriptPath))
            {
                MessageBox.Show(monacoEditorScriptPath, "Папки не существует", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
#endif

            if (Directory.Exists(folderPath))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping("app.local", folderPath, CoreWebView2HostResourceAccessKind.Allow);
            }

            if (Directory.Exists(monacoEditorScriptPath))
            {
                webView.CoreWebView2.SetVirtualHostNameToFolderMapping("monaco-editor-scripts", monacoEditorScriptPath, CoreWebView2HostResourceAccessKind.Allow);
            }

            webView.CoreWebView2.WebMessageReceived += WebView_WebMessageReceived;
            webView.Source = new Uri($"https://app.local/index.html");
        }

        private void WebView_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                var ev = JsonSerializer.Deserialize<JsEventData>(e.WebMessageAsJson);
                if (String.Equals(ev?.action, "init-monaco-editor"))
                {
                    taskInitMonacoEditor.SetResult(true);
                }
                else if (String.Equals(ev?.action, "save-content"))
                {
                    SaveContent();
                }
            }
            catch(Exception ex)
            {

            }
        }

        private void SaveContent()
        {
            if (taskInitMonacoEditor.Task.Result)
            {
                NameLeftTools.SetTextInfo("Получаю контент для сохранения...");
                var awaiter = webView.ExecuteScriptAsync("assistant.getModifiedText()").GetAwaiter();
                awaiter.OnCompleted(() =>
                {
                    Assistant.StartAsync(() =>
                    {
                        NameLeftTools.SetTextInfo("Сохраняю...");
                        var text = awaiter.GetResult();
                        text = JsonSerializer.Deserialize<string>(text);
                        File.WriteAllText(LocalFile, text);
                        ReadContent(NameLeftTools.CurrentBranch?.Name);
                        ShowContent();
                        NameLeftTools.SetTextInfo("OK");
                    });
                });
            }
        }

        private void OnChangeBranch(GitBranchInfo branch)
        {
            if (!String.IsNullOrWhiteSpace(branch?.Name))
            {
                Assistant.StartAsync(() =>
                {
                    NameLeftTools.SetTextInfo("Читаю Git...");
                    ReadContent(branch?.Name);
                    NameLeftTools.SetTextInfo("OK");
                }).Then((x) =>
                {
                    ShowContent();
                });
            }
        }
    }

    public class JsEventData
    {
        public string action { set; get; }
    }
}
