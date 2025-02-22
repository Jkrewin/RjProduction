using Microsoft.Win32;
using RjProduction.Fgis.XML;
using System.IO;
using System.Text;
using System.Windows;
using static RjProduction.Fgis.XML.forestUsageReport.AttachmentListType.AttachmentType;
using static RjProduction.Fgis.XML.forestUsageReport.AttachmentListType;

namespace RjProduction.Fgis.WPF
{
    
    public partial class DialogAttachment : Window
    {
        private Action<AttachmentType> _action;
        private AttachmentType _AttachmentType;

        public DialogAttachment(AttachmentType attachmentType, Action<AttachmentType> action)
        {
            InitializeComponent();
            this._AttachmentType = attachmentType;
            _action = action;
        }

        private void ПринятьИзменения(object sender, RoutedEventArgs e)
        {
            if (File1s.Tag is null) {
                MessageBox.Show("Вы не выбрали файл который нужно прикрепить. ");
                return;
            }
            _AttachmentType.desc = TBox_desc.Text;
            _AttachmentType.id = "id_" + Convert.ToHexString(System.Security.Cryptography.MD5.HashData(Encoding.ASCII.GetBytes(DateTime.Now.ToString())));

            Mod? mod= Mod.Extract_Mod(_AttachmentType.GetType().GetProperty("id")!);
            if (mod is null || Mod.CheckRule(_AttachmentType.id, mod) ==false) {
                throw new Exception("id не соответствует правилу");
            }

            AttachmentFileType attachmentFile1 = new ()
            {
                FilePatch = (string)File1s.Tag, 
                fileURI= (string)File1s.Content
            };

            AttachmentFileType? attachmentFile2;
            if (File2s.Tag is not null)
            {
                attachmentFile2 = new()
                {
                    FilePatch = (string)File2s.Tag,
                    fileURI = (string)File2s.Content
                };
                _AttachmentType.signature = attachmentFile2;
            }
            _AttachmentType.file = attachmentFile1;
            _action(_AttachmentType);
            ЗакрытьОкно(null!, null!);
        }      

        private void OpenFileDialog_Open(System.Windows.Controls.Label label) {
            // Создаем диалоговое окно для выбора файла
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Выберите файл",
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "",
                Filter = "Все файлы (*.*)|*.*",
                FilterIndex = 2,
                RestoreDirectory = true
            };

            // Показываем диалоговое окно и проверяем результат
            if (openFileDialog.ShowDialog()==true  )
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);
                label.Content= fileInfo.Name;
                label.Tag = openFileDialog.FileName;
            }
        }

        private void ЗакрытьОкно(object sender, RoutedEventArgs e)=> this.Close();

        private void Изменить_Путь(object sender, RoutedEventArgs e)
        {
            OpenFileDialog_Open(File1s);
            string file = $"{File1s.Tag}.sig";
            if (File.Exists(file))
            {
                FileInfo fileInfo = new(file);
                File2s.Content = fileInfo.Name;
                File2s.Tag = file;
            }
        }

        private void Изменить_Путь2(object sender, RoutedEventArgs e)
        {
            OpenFileDialog_Open(File2s);
            FileInfo fileInfo = new FileInfo((string)File2s.Tag);
            if (fileInfo.Extension != ".sig")
            {
                if (MessageBox.Show("Проверите действительно этот файл является подписью к этому файлу. Да это верно ?", "", MessageBoxButton.YesNo) == MessageBoxResult.No)
                {
                    File2s.Content = "<Выберете другой файл>";
                    File2s.Tag = "";
                }
            }
        }
    }
}
