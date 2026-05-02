using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Views;

namespace BRB6.Template;

public partial class SaveChangesPopup : Popup
{
	public SaveChangesPopup()
	{
		InitializeComponent();
	}

    void OnSave(object sender, EventArgs e) => Close(true);
    void OnDiscard(object sender, EventArgs e) => Close(false);
}