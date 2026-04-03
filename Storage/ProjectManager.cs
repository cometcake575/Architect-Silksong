using Silksong.ModMenu;
using Silksong.ModMenu.Elements;
using Silksong.ModMenu.Models;
using Silksong.ModMenu.Screens;

namespace Architect.Storage;

public class ProjectManager
{
    public static void Init()
    {
        Registry.AddModMenu("Architect Control", () =>
        {
            var tb = new TextButton(LocalizedText.Raw("Architect Project"));
            var current = "";
            
            var sms = new SimpleMenuScreen("Project Manager");
            var te = new TextInput<string>(LocalizedText.Raw("Project To Save"), new ParserTextModel<string>(
                (p, out q) =>
                {
                    q = p;
                    return true;
                }, (p, out q) =>
                {
                    q = p;
                    return true;
                }, current));
            te.OnValueChanged += s => current = s;
            sms.Add(te);

            var save = new TextButton("Save");
            sms.Add(save);
            
            var choice = new ChoiceElement<string>("Project To Load", ["None"]);
            sms.Add(choice);
            
            var load = new TextButton("Load");
            sms.Add(load);
            
            var delete = new TextButton("Delete");
            sms.Add(delete);
            
            save.OnSubmit += () =>
            {
                GlobalArchitectData.Instance.SavedMapNames.Add(te.Value);
                StorageManager.MakeBackup(te.Value);
                UpdateValues();
            };
            
            load.OnSubmit += () =>
            {
                if (choice.Value == "None") return;
                StorageManager.LoadBackup(choice.Value);
            };
            
            delete.OnSubmit += () =>
            {
                if (choice.Value == "None") return;
                GlobalArchitectData.Instance.SavedMapNames.Remove(choice.Value);
                StorageManager.DeleteBackup(choice.Value);
                UpdateValues();
            };
            
            tb.OnSubmit += () =>
            {
                UpdateValues();
                MenuScreenNavigation.Show(sms);
            };
            return tb;

            void UpdateValues()
            {
                ((ListChoiceModel<string>)choice.ChoiceModel)
                    .UpdateValues(
                        GlobalArchitectData.Instance.SavedMapNames.IsNullOrEmpty() ? ["None"] : 
                        GlobalArchitectData.Instance.SavedMapNames, 0);
            }
        });
    }
}