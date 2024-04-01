using System;

using System.Collections.Generic;

using System.ComponentModel;

using System.DirectoryServices;

using System.DirectoryServices.AccountManagement;

using System.DirectoryServices.ActiveDirectory;

using System.Security;

using System.Security.AccessControl;

using System.Security.Cryptography.X509Certificates;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Controls.Primitives;

using System.Windows.Documents;

using System.Windows.Input;

using System.Windows.Media;



namespace AppliADWPF





    //By Matthis Heritier/ROY

{

    public partial class accueil : Window

    {







        //code By Matthis Heritier/Roy

        private string domain = "grm.lan";

        private List<UserResult> utilisateursRecherches = new List<UserResult>();

        private bool isResizing = false;

        private Point resizeStartPoint;

        private double initialWidth;

        private double initialHeight;

        





        public accueil()

        {

            InitializeComponent();

            txtSearch.KeyUp += TxtSearch_KeyUp;

            ListBoxUsers.AddHandler(ListBoxItem.MouseDownEvent, new MouseButtonEventHandler(UserResult_MouseDown));



            this.Loaded += accueil_Loaded;

            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;

            this.SizeChanged += accueil_SizeChanged;

            this.MouseMove += accueil_MouseMove;

            this.MouseLeftButtonUp += accueil_MouseLeftButtonUp;

        }

      

        private void accueil_Loaded(object sender, RoutedEventArgs e)

        {

            this.SizeChanged += accueil_SizeChanged;

            TextBoxUserDetails.FontWeight = FontWeights.Bold;



        }



        







        private void accueil_SizeChanged(object sender, SizeChangedEventArgs e)

        {

            gridaccueil.Width = this.ActualWidth;

            gridaccueil.Height = this.ActualHeight;

        }



        private void accueil_MouseMove(object sender, MouseEventArgs e)

        {

            if (isResizing)

            {

                Point currentPoint = e.GetPosition(this);

                double deltaX = currentPoint.X - resizeStartPoint.X;

                double deltaY = currentPoint.Y - resizeStartPoint.Y;

                double newWidth = initialWidth + deltaX;

                double newHeight = initialHeight + deltaY;



                if (newWidth > 0 && newHeight > 0)

                {

                    this.Width = newWidth;

                    this.Height = newHeight;

                }

            }

        }

        private void ListBoxUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {

            // Votre code de gestion de la sélection changée ici

        }



        private void accueil_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)

        {

            isResizing = false;

            this.ReleaseMouseCapture();

        }



        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            if (e.LeftButton == MouseButtonState.Pressed)

            {

                DragMove();

            }

        }

        private void BorderResize_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            isResizing = true;

            resizeStartPoint = e.GetPosition(this);

            initialWidth = this.Width;

            initialHeight = this.Height;

            this.CaptureMouse();

        }









        private void TxtSearch_KeyUp(object sender, KeyEventArgs e)

        {

            if (e.Key == Key.Enter)

            {

                SearchButton_Click(sender, e);

            }

        }



        private void UserResult_MouseDown(object sender, MouseButtonEventArgs e)

        {

            if (sender is TextBlock textBlock && textBlock.DataContext is UserResult userResult)

            {

                TextBoxUserDetails.Text = userResult.UserDetails;



                ListBoxCDS.Items.Clear();

                ListBoxGroupes.Items.Clear();



                foreach (string group in userResult.UserGroups)

                {

                    if (group.StartsWith("GR_", StringComparison.OrdinalIgnoreCase))

                    {

                        ListBoxCDS.Items.Add(group);

                    }

                    else if (group.StartsWith("GP_", StringComparison.OrdinalIgnoreCase))

                    {

                        ListBoxGroupes.Items.Add(group);

                    }

                    else

                    {

                        List<string> subgroups = RetrieveSubgroups(group, domain);

                        foreach (string subgroup in subgroups)

                        {

                            ListBoxGroupes.Items.Add(subgroup);

                        }

                    }

                }

            }

        }







        private void Window_MouseDown(object sender, MouseButtonEventArgs e)

        {

            if (e.LeftButton == MouseButtonState.Pressed)

                DragMove();

        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            if (e.LeftButton == MouseButtonState.Pressed)

                DragMove();

        }







        private void btnMinimize_Click(object sender, RoutedEventArgs e)

        {

            WindowState = WindowState.Minimized;

        }



        private void btnClose_Click(object sender, RoutedEventArgs e)

        {

            this.Close();

        }



        private void SearchButton_Click(object sender, RoutedEventArgs e)

        {

            string lastName = txtSearch.Text;



            

            ListBoxUsers.ItemsSource = null;

            TextBoxUserDetails.Text = string.Empty;

            



            List<UserResult> nouveauxUtilisateursRecherches = RechercherUtilisateursAD(lastName);

            ListBoxUsers.ItemsSource = nouveauxUtilisateursRecherches;

            utilisateursRecherches.Clear();

            utilisateursRecherches.AddRange(nouveauxUtilisateursRecherches);



            foreach (UserResult userResult in utilisateursRecherches)

            {

                foreach (string group in userResult.UserGroups)

                {

                    if (group.StartsWith("GR_", StringComparison.OrdinalIgnoreCase))

                    {

                        if (!ListBoxCDS.Items.Contains(group))

                        {

                            ListBoxCDS.Items.Add(group);

                        }

                    }



                    

                }

            }

        }











        private class UserResult

        {

            public string DisplayName { get; set; }

            public string UserDetails { get; set; }

            public List<string> UserGroups { get; set; }

            public override string ToString()

            {

                return DisplayName;

            }

        }



        private List<UserResult> RechercherUtilisateursAD(string nom)

        {

            List<UserResult> utilisateurs = new List<UserResult>();



            using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{domain}"))

            {

                using (DirectorySearcher searcher = new DirectorySearcher(entry))

                {

                    if (!string.IsNullOrEmpty(nom))

                    {

                        searcher.Filter = $"(&(objectCategory=user)(displayName=*{nom}*))";

                    }

                    else

                    {

                        searcher.Filter = "(objectCategory=user)";

                    }



                    searcher.PropertiesToLoad.AddRange(new string[]

                    {

                "sAMAccountName",

                "displayName",

                "mail",

                "title",

                "company",

                "streetAddress",

                "description",

                "telephoneNumber",

                "userAccountControl",

                "pwdLastSet",

                "lastLogon",

                "memberOf"

                    });



                    SearchResultCollection results = searcher.FindAll();



                    foreach (SearchResult result in results)

                    {

                        DirectoryEntry userEntry = result.GetDirectoryEntry();



                        string displayName = userEntry.Properties["displayName"].Value?.ToString() ?? string.Empty;

                        string userAccountControl = ((int)userEntry.Properties["userAccountControl"].Value & 0x2) == 0 ? "Actif" : "Inactif";

                        string pwdLastSet = GetPwdLastSet(userEntry.Properties["pwdLastSet"].Value) ?? string.Empty;

                        string lastLogon = GetLastLogon(userEntry.Properties["lastLogon"].Value) ?? string.Empty;



                        TextBlock textBlock = new TextBlock();

                        textBlock.Inlines.Add(new Bold(new Run("Nom : ")));

                        textBlock.Inlines.Add(displayName);



                        string userDetails = $"UserName:         {userEntry.Properties["sAMAccountName"].Value}\n" +

                            $"Nom-Prénom:    {displayName}\n" +

                            $"------------------------------{userEntry.Properties[""].Value}\n" +

                            $"E-mail:                                  {userEntry.Properties["mail"].Value}\n" +

                            $"Titre:                                     {userEntry.Properties["title"].Value}\n" +

                            $"Entreprise:                            {userEntry.Properties["company"].Value}\n" +

                            $"Numéro de tél:                                  {userEntry.Properties["telephoneNumber"].Value}\n" +

                            $"Mobile:                                  {userEntry.Properties["mobile"].Value}\n"+

                            $"------------------------------{userEntry.Properties[""].Value}\n" +

                            $"Actif:                                        {userAccountControl}\n" +

                            $"Dernier changement MDP :   {pwdLastSet}\n" +

                            $"Dernière connexion :              {lastLogon}\n";



                        List<string> userGroups = new List<string>();

                        if (userEntry.Properties.Contains("memberOf"))

                        {

                            foreach (var group in userEntry.Properties["memberOf"])

                            {

                                string groupName = GetGroupNameFromDistinguishedName(group.ToString());

                                if (groupName.StartsWith("GR_", StringComparison.OrdinalIgnoreCase))

                                {

                                    userGroups.Add(groupName);

                                    List<string> subgroups = RetrieveSubgroups(groupName, domain);

                                    userGroups.AddRange(subgroups);

                                }

                            }

                        }



                        UserResult userResult = new UserResult()

                        {

                            DisplayName = displayName,

                            UserDetails = userDetails,

                            UserGroups = userGroups

                        };



                        utilisateurs.Add(userResult);

                    }

                }

            }



            return utilisateurs;

        }





        private string GetPwdLastSet(object pwdLastSet)

        {

            if (pwdLastSet == null)

                return "Non défini";



            long pwdLastSetTicks = ConvertADSLargeIntegerToInt64(pwdLastSet);

            DateTime pwdLastSetDateTime = DateTime.FromFileTime(pwdLastSetTicks);

            return pwdLastSetDateTime.ToString();

        }



        private string GetLastLogon(object lastLogon)

        {

            if (lastLogon == null)

                return "Non défini";



            long lastLogonTicks = ConvertADSLargeIntegerToInt64(lastLogon);

            DateTime lastLogonDateTime = DateTime.FromFileTime(lastLogonTicks);

            return lastLogonDateTime.ToString();

        }



        public static long ConvertADSLargeIntegerToInt64(object adsLargeInteger)

        {

            var highPart = (int)adsLargeInteger.GetType().InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, adsLargeInteger, null);

            var lowPart = (int)adsLargeInteger.GetType().InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty, null, adsLargeInteger, null);

            return ((long)highPart << 32) + (uint)lowPart;

        }



        static string GetGroupNameFromDistinguishedName(string distinguishedName)

        {

            int startIndex = distinguishedName.IndexOf("CN=", StringComparison.OrdinalIgnoreCase);

            int endIndex = distinguishedName.IndexOf(",", StringComparison.OrdinalIgnoreCase);

            if (startIndex >= 0 && endIndex > startIndex)

            {

                return distinguishedName.Substring(startIndex + 3, endIndex - startIndex - 3);

            }

            return distinguishedName;

        }



        private List<string> RetrieveSubgroups(string groupName, string domain)

        {

            List<string> subgroups = new List<string>();



            using (DirectoryEntry entry = new DirectoryEntry($"LDAP://{domain}"))

            {

                using (DirectorySearcher searcher = new DirectorySearcher(entry))

                {

                    searcher.Filter = $"(&(objectCategory=group)(cn={groupName}))";

                    searcher.PropertiesToLoad.Add("memberOf");



                    SearchResultCollection results = searcher.FindAll();



                    if (results.Count > 0)

                    {

                        foreach (SearchResult result in results)

                        {

                            DirectoryEntry groupEntry = result.GetDirectoryEntry();



                            if (groupEntry.Properties.Contains("memberOf"))

                            {

                                foreach (var subgroup in groupEntry.Properties["memberOf"])

                                {

                                    string subgroupName = GetGroupNameFromDistinguishedName(subgroup.ToString());

                                    subgroups.Add(subgroupName);

                                    subgroups.AddRange(RetrieveSubgroups(subgroupName, domain));

                                }

                            }

                        }

                    }

                }

            }



            return subgroups;

        }



        private void ListBoxDescription_SelectionChanged(object sender, SelectionChangedEventArgs e)

        {



        }



        private void ListBoxDescription_SelectionChanged_1(object sender, SelectionChangedEventArgs e)

        {



        }



    }

}