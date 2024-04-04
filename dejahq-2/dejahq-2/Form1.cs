using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using System.Xml.Linq;
using static dejahq_2.Form1;

namespace dejahq_2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private WorkOrder currentWorkOrder;

        
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Szövegfájlok|*.txt";
            openFileDialog.Title = "Adatok betöltése";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = openFileDialog.FileName;
               currentWorkOrder=LoadWorksFromFile(filename);
            }
        }

        private WorkOrder LoadWorksFromFile(string filename)
        {
            //try
            //{
            //    List<Work> works = new List<Work>();

            //    string[] lines = File.ReadAllLines(filename);
            //    foreach (string line in lines)
            //    {
            //        string[] parts = line.Split(';');
            //        Work work = new Work
            //        {
            //            Name = parts[0],
            //            RequiredHours = int.Parse(parts[1]),
            //            MaterialCost = int.Parse(parts[2])
            //        };
            //        works.Add(work);
            //    }

            //    // Betöltött munkák hozzáadása a rendszerhez
            //    // Ebben a példában egyszerűen csak kiíratom a munkák számát
            //    MessageBox.Show($"Sikeresen betöltve {works.Count} munka.", "Sikeres betöltés", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show($"Hiba történt a fájl betöltése közben: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            try
            {
                List<Work> works = new List<Work>();

                string[] lines = File.ReadAllLines(filename);
                foreach (string line in lines)
                {
                    string[] parts = line.Split(';');
                    Work work = new Work
                    {
                        Name = parts[0],
                        RequiredHours = int.Parse(parts[1]),
                        MaterialCost = int.Parse(parts[2])
                    };
                    works.Add(work);
                }

                MessageBox.Show($"Sikeresen betöltve {works.Count} munka.", "Sikeres betöltés", MessageBoxButtons.OK, MessageBoxIcon.Information);

                WorkOrder workOrder = new WorkOrder();
                workOrder.Works = works;
                return workOrder;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a fájl betöltése közben: {ex.Message}", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void ExitApplication(object sender, EventArgs e)
        {
            // Kilépés megerősítése
            DialogResult result = MessageBox.Show("Biztosan kilép?", "Kilépés", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        public class Work
        {
            public string Name { get; set; }
            public int RequiredHours { get; set; }
            public int MaterialCost { get; set; }

            // Munkaidő számított tulajdonság
            public int TotalHours => RequiredHours / 60;
            public int TotalMinutes => RequiredHours % 60;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //if (currentWorkOrder != null)
            //{
            //    //DialogResult result = MessageBox.Show("Már van aktív munkalap. Szeretnél újat kezdeni?", "Új munkalap kezdése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //    //if (result == DialogResult.No)
            //    //{
            //    //    return;
            //    //}
            //    OrderForm orderForm1 = new OrderForm();
            //    orderForm1.Works = LoadWorksFromFile(); // Betöltjük a munkákat
            //    if (orderForm1.ShowDialog() == DialogResult.OK)
            //    {
            //        currentWorkOrder = orderForm1.WorkOrder; // Frissítjük a jelenlegi munkalapot
            //    }
            //}

            //OrderForm orderForm = new OrderForm();
            //orderForm.Works = currentWorkOrder?.Works;
            //if (orderForm.ShowDialog() == DialogResult.OK)
            //{
            //    currentWorkOrder = orderForm.WorkOrder;
            //}
            //if (currentWorkOrder != null)
            //{
            //    OrderForm orderForm1 = new OrderForm(currentWorkOrder.Works);
            //    /*orderForm1.Works = LoadWorksFromFile();*/ // Betöltjük a munkákat
            //    if (orderForm1.ShowDialog() == DialogResult.OK)
            //    {
            //        currentWorkOrder = orderForm1.WorkOrder; // Frissítjük a jelenlegi munkalapot
            //    }
            //}
            //else
            //{
            //    OrderForm orderForm = new OrderForm();
            //   /* orderForm.Works = LoadWorksFromFile();*/ // Betöltjük a munkákat
            //    if (orderForm.ShowDialog() == DialogResult.OK)
            //    {
            //        currentWorkOrder = orderForm.WorkOrder;
            //    }
            //}
            // Új munkalap ablak létrehozása
            OrderForm orderForm = new OrderForm(currentWorkOrder?.Works);

            // Rögzítés eseménykezelő hozzáadása
            orderForm.OrderSubmitted += OrderForm_OrderSubmitted;

            // Az ablak megjelenítése
            if (orderForm.ShowDialog() == DialogResult.OK)
            {
                currentWorkOrder = orderForm.WorkOrder;
            }

        }
        private void OrderForm_OrderSubmitted(object sender, EventArgs e)
        {
            if (currentWorkOrder == null)
            {
                currentWorkOrder = new WorkOrder();
            }

            // Ellenőrizd, hogy a sender objektum egy OrderForm példány-e
            if (sender is OrderForm orderForm)
            {
                // Ellenőrizd, hogy van-e kiválasztott munka az OrderForm-ban
                if (orderForm.Works != null && orderForm.Works.Count > 0)
                {
                    // Minden kiválasztott munkát hozzáadunk az aktuális munkalaphoz
                    foreach (var work in orderForm.Works)
                    {
                        currentWorkOrder.AddWork(work);
                    }

                    // Munkadíj és anyagköltség kiszámítása
                    int totalMaterialCost = 0;
                    int totalLaborCost = 0;
                    foreach (var work in orderForm.Works)
                    {
                        totalMaterialCost += work.MaterialCost;
                        // Munkadíj számítása: 1 óra = 15000 Ft, minden megkezdett fél óra = 2500 Ft
                        totalLaborCost += 15000 + (work.RequiredHours / 30) * 2500;
                    }

                    // Az aktuális munkalap adatainak frissítése
                    currentWorkOrder.TotalMaterialCost = totalMaterialCost;
                    currentWorkOrder.TotalLaborCost = totalLaborCost;
                    currentWorkOrder.TotalHours = orderForm.Works.Sum(work => work.RequiredHours);

                    // Egyéb műveletek (pl. adatbázisba mentés)
                    // ...

                    // Sikeres rögzítés üzenet megjelenítése
                    MessageBox.Show("A munkalap sikeresen rögzítve!", "Rögzítés", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Ha nincs kiválasztott munka az OrderForm-ban, akkor csak üzenetet jelenítünk meg
                    MessageBox.Show("Nincs kiválasztott munka a munkalapban!", "Figyelmeztetés", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Munkák számának meghatározása
            int numberOfWorks = currentWorkOrder != null ? currentWorkOrder.Works.Count : 0;

            // Anyagköltség meghatározása
            int totalMaterialCost = currentWorkOrder != null ? currentWorkOrder.TotalMaterialCost : 0;

            // Munkadíj meghatározása
            int totalLaborCost = currentWorkOrder != null ? currentWorkOrder.TotalLaborCost : 0;

            // Az anyagköltség és munkadíj összege
            int totalCost = totalMaterialCost + totalLaborCost;
            MessageBox.Show($"Munkák száma: {numberOfWorks}" +
                $"\nAnyagköltség: {totalMaterialCost}\nMunkadíj: {totalLaborCost}" +
                $"\nÖsszes költség: {totalCost}", "Fizetés", MessageBoxButtons.OK);
        }
        private List<Work> LoadWorksFromFile()
        {
            List<Work> works = new List<Work>();
            // Ide írd meg a fájlból történő munkák betöltését
            // Például:
             works = LoadWorksFromFile();
            return works;
        }

        // Munkalap osztály
        public class WorkOrder
        {
            public List<Work> Works { get; set; }
            public int TotalMaterialCost { get; set; }
            public int TotalLaborCost { get; set; }
            public int TotalHours { get; set; }

            public WorkOrder()
            {
                Works = new List<Work>();
            }

            // Munka hozzáadása a munkalaphoz
            public void AddWork(Work work)
            {
                Works.Add(work);
                TotalMaterialCost += work.MaterialCost;
                TotalLaborCost += 15000; // Állandó munkaóradíj
                TotalHours += work.RequiredHours;
            }

            //private void button2_Click(object sender, EventArgs e, object currentWorkOrder)
            //{
            //    if (currentWorkOrder != null)
            //    {
            //        DialogResult result = MessageBox.Show("Már van aktív munkalap. Szeretnél újat kezdeni?", "Új munkalap kezdése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            //        if (result == DialogResult.No)
            //        {
            //            return;
            //        }
            //    }

            //    // Munkalap ablak létrehozása és megjelenítése
            //    OrderForm orderForm = new OrderForm();
            //    // Ha van aktív munkalap, átadjuk a munkákat
            //    if (orderForm.ShowDialog() == DialogResult.OK)
            //    {
            //        currentWorkOrder = orderForm.WorkOrder; // A megjelenített munkalap frissítése
            //    }
            //}


        }
        public class OrderForm : Form
        {
            private TableLayoutPanel layoutPanel;
            public List<Work> Works { get; set; } // A rendelkezésre álló munkák

            public WorkOrder WorkOrder { get; private set; } // Létrehozott munkalap
            public event EventHandler OrderSubmitted;

            // Konstruktor
            public OrderForm(List<Work> works=null)
            {
                Works=works;
                InitializeUI();
            }

            private void InitializeUI()
            {
                //    if (Works != null)
                //    {
                //        int y = 20;
                //        foreach (Work work in Works)
                //        {
                //            CheckBox checkBox = new CheckBox();
                //            checkBox.Text = $"{work.Name} - {work.RequiredHours / 60} óra {work.RequiredHours % 60} perc";
                //            checkBox.Location = new Point(20, y);
                //            checkBox.Tag = work;
                //            this.Controls.Add(checkBox);
                //            y += 25;
                //        }
                //    }

                //    Button createButton = new Button();
                //    createButton.Text = "Rögzítés";
                //    createButton.Location = new Point(20, this.Height - 70);
                //    createButton.Click += CreateOrder;
                //    this.Controls.Add(createButton);

                //    Button cancelButton = new Button();
                //    cancelButton.Text = "Mégsem";
                //    cancelButton.Location = new Point(150, this.Height - 70);
                //    cancelButton.Click += CancelOrder;
                //    this.Controls.Add(cancelButton);
                // Elrendezés inicializálása
                layoutPanel = new TableLayoutPanel();
                layoutPanel.Dock = DockStyle.Fill;
                layoutPanel.AutoScroll = true;
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
                layoutPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

                // Munkalap címkék hozzáadása
                int rowIndex = 0;
                foreach (Work work in Works)
                {
                    Label nameLabel = new Label();
                    nameLabel.Text = work.Name;
                    layoutPanel.Controls.Add(nameLabel, 0, rowIndex);

                    Label timeLabel = new Label();
                    timeLabel.Text = $"{work.RequiredHours / 60} óra {work.RequiredHours % 60} perc";
                    layoutPanel.Controls.Add(timeLabel, 1, rowIndex);

                    Label costTextBox = new Label();
                    costTextBox.Text = "15000"; // Munkadíj inicializálása
                    layoutPanel.Controls.Add(costTextBox, 3, rowIndex);

                    CheckBox checkBox = new CheckBox();
                    checkBox.Tag = work;
                    layoutPanel.Controls.Add(checkBox, 2, rowIndex);

                    rowIndex++;
                }

                // Anyagköltség címke és mező hozzáadása
                Label materialCostLabel = new Label();
                materialCostLabel.Text = "Anyagköltség:";
                layoutPanel.Controls.Add(materialCostLabel, 0, rowIndex);

                Label materialCostValueLabel = new Label();
                materialCostValueLabel.Text = "0";
                layoutPanel.Controls.Add(materialCostValueLabel, 1, rowIndex);

                // Munkadíj címke és mező hozzáadása
                Label laborCostLabel = new Label();
                laborCostLabel.Text = "Munkadíj:";
                layoutPanel.Controls.Add(laborCostLabel, 2, rowIndex);

                Label laborCostValueLabel = new Label();
                laborCostValueLabel.Text = "0";
                layoutPanel.Controls.Add(laborCostValueLabel, 3, rowIndex);

                // Rögzítés gomb hozzáadása
                Button submitButton = new Button();
                submitButton.Text = "Rögzítés";
                submitButton.Click += SubmitButton_Click;
                layoutPanel.Controls.Add(submitButton, 0, rowIndex + 1);

                // LayoutPanel hozzáadása a Form-hoz
                this.Controls.Add(layoutPanel);
                foreach (Control control in layoutPanel.Controls)
                {
                    if(control is CheckBox checkBox)
                    {
                        checkBox.CheckedChanged += (sender, e) =>
                        {
                            UpdateTotalCosts();
                        };
                    }
                }
            }

            private void UpdateTotalCosts()
            {
                int materialCost = 0;
                int laborCost = 0;
                foreach (Control control in layoutPanel.Controls)
                {
                    if(control is CheckBox checkBox && checkBox.Checked)
                    {
                        Work selectedWork=checkBox.Tag as Work; 
                        materialCost += selectedWork.MaterialCost;
                        laborCost += 15000;
                    }
                }
                (layoutPanel.Controls[layoutPanel.Controls.Count-4] as Label).Text = materialCost.ToString();
                (layoutPanel.Controls[layoutPanel.Controls.Count-2] as Label).Text = laborCost.ToString();
            }

            private void SubmitButton_Click(object sender, EventArgs e)
            {
                //int materialCost = 0;
                //int laborCost = 0;
                //try
                //{
                //    if (layoutPanel != null)
                //    {
                //        foreach (Control control in layoutPanel.Controls)
                //        {
                //            if (control is CheckBox checkBox && checkBox.Checked)
                //            {
                //                Work selectedWork = checkBox.Tag as Work;
                //                materialCost += selectedWork.MaterialCost;
                //                laborCost += int.Parse((control.Parent.Controls[2] as TextBox).Text);
                //            }
                //        }
                //        (layoutPanel.Controls[layoutPanel.Controls.Count - 4] as Label).Text = materialCost.ToString();
                //        (layoutPanel.Controls[layoutPanel.Controls.Count - 2] as Label).Text = laborCost.ToString();

                //    }
                //    else
                //    {
                //        MessageBox.Show("A layoutPanel objektum null értékű", "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show($"Hiba történt a Submit gomb eseménykezelőjében: {ex.Message}", "Hiba", MessageBoxButtons.OK,MessageBoxIcon.Error);
                //}
                if(OrderSubmitted!=null)
                   OrderSubmitted(this, EventArgs.Empty);

                this.Close();
                
                


            }
            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                if(e.CloseReason == CloseReason.UserClosing && OrderSubmitted == null)
                {
                    DialogResult result = MessageBox.Show("Biztosan bezárja az ablakot rögzítés nélkül? A munkalap változásai elvesznek.", 
                        "Figyelmeztetés",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if(result!=DialogResult.Yes)
                    {
                        e.Cancel = true;
                    }
                }
                base.OnFormClosing(e);
            }

            private void button3_Click(object sender, EventArgs e)
            {
                int workCount=layoutPanel.Controls.OfType<CheckBox>().Count(cb=>cb.Checked);
                int totalCost = int.Parse((layoutPanel.Controls[layoutPanel.Controls.Count - 4] as Label).Text) +
                                int.Parse((layoutPanel.Controls[layoutPanel.Controls.Count - 2] as Label).Text);
                MessageBox.Show($"Munkák száma: {workCount}\nAnyagköltség + Munkadíj: {totalCost}", "Fizetés", MessageBoxButtons.OK);
            }
            private void CreateOrder(object sender, EventArgs e)
            {
                WorkOrder = new WorkOrder();
                foreach (Control control in this.Controls)
                {
                    if (control is CheckBox checkBox && checkBox.Checked)
                    {
                        Work selectedWork = checkBox.Tag as Work;
                        WorkOrder.AddWork(selectedWork);
                    }
                }

                if (WorkOrder.Works.Count > 0)
                {
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Legalább egy munkát ki kell választani a munkalap létrehozásához.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            private void CancelOrder(object sender, EventArgs e)
            {
                DialogResult result = MessageBox.Show("Biztosan megszakítja a munkalap készítést? A munkalap törlődni fog.", "Munkalap törlése", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    this.DialogResult = DialogResult.Cancel;
                }
            }
            private void button1_Click(object sender, EventArgs e)
            {
                WorkOrder = new WorkOrder();
                foreach (Control control in this.Controls)
                {
                    if (control is CheckBox checkBox && checkBox.Checked)
                    {
                        Work selectedWork = checkBox.Tag as Work;
                        WorkOrder.AddWork(selectedWork);
                    }
                }

                if (WorkOrder.Works.Count > 0)
                {
                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("Legalább egy munkát ki kell választani a munkalap létrehozásához.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            //private void button2_Click(object sender, EventArgs e)
            //{
            //    // Kiválasztott munkák alapján munkalap létrehozása
            //    //WorkOrder = new WorkOrder();
            //    //foreach (Control control in this.Controls)
            //    //{
            //    //    if (control is CheckBox checkBox && checkBox.Checked)
            //    //    {
            //    //        Work selectedWork = checkBox.Tag as Work;
            //    //        WorkOrder.AddWork(selectedWork);
            //    //    }
            //    //}

            //    //// Ha legalább egy munka ki lett választva, akkor OK eredményt adjunk vissza
            //    //if (WorkOrder.Works.Count > 0)
            //    //{
            //    //    this.DialogResult = DialogResult.OK;
            //    //}
            //    //else
            //    //{
            //    //    MessageBox.Show("Legalább egy munkát ki kell választani a munkalap létrehozásához.", "Hiányzó adat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    //}
            //    this.DialogResult = DialogResult.Cancel;
            //}
            //private void CancelOrder(object sender, EventArgs e)
            //{
            //    this.DialogResult = DialogResult.Cancel;
            //}
        }
        //---------------------------------------------Névjegy

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"{DateTime.Now}\nDEJAHQ", "Névjegy", MessageBoxButtons.OK);
        

        }
        //-----------------------------------------------
        //-----------------------------------------------Kilépés
        private void button5_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Biztosan ki akar lépni?", "Figyelem", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }


        //-----------------------------------------------
        
    }
}
