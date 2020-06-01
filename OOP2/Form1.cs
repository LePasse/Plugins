using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.IO;
using PlugIn;

namespace OOP2
{
    using Classes;
    using Serial;

    public partial class Form1 : Form, IHost
    {
        List<Control> controls = new List<Control>();
        List<Object> allObj = new List<Object>();

        private PluginManager pm = new PluginManager();
        public Form1()
        {
            InitializeComponent();
            String s = AppDomain.CurrentDomain.BaseDirectory + "Plugins\\";
            //MessageBox.Show(s);
            pm.ScanPlugins(s);
            foreach (var plugin in pm.Plugins)
            {
                var item = comboBox4.Items.Add(plugin.Name);
                //MessageBox.Show(plugin.Name);
            }
            if (comboBox4.Items.Count > 0)
            {
                comboBox4.SelectedIndex = 0;
            }
        }
        public void AddComboBox(Control ctrl)
        {
            this.Controls.Add(ctrl);
        }
        public class PluginManager
        {
            public List<IPlugin> Plugins = new List<IPlugin>();
            public void ScanPlugins(string directory)
            {
                foreach (var file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
                    try
                    {
                        //MessageBox.Show(file.ToString());
                        var ass = Assembly.LoadFile(file);
                        foreach (var type in ass.GetTypes())
                        {
                            var i = type.GetInterface("IPlugin");
                            //MessageBox.Show(i.ToString());
                            if (i != null)
                                Plugins.Add(ass.CreateInstance(type.FullName) as IPlugin);
                        }

                    }
                    catch {/*is not .NET assembly*/}
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Type[] typelist = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Classes").ToArray();
            foreach (Type type in typelist)
            {
                if (type.IsClass)
                {
                    comboBox1.Items.Add(type.Name);
                }
            }

            Type[] typelist2 = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.Namespace == "Serial").ToArray();
            foreach (Type type in typelist2)
            {
                if (type.IsClass)
                {
                    comboBox3.Items.Add(type.Name);
                }
            }
        }

        public static Object create_obj(Type type)
        {
            ConstructorInfo[] cons = type.GetConstructors();
            ParameterInfo[] pars = cons[0].GetParameters();
            List<Object> test = new List<Object>();
            if (pars.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                foreach (var para in pars)
                {
                    test.Add(create_obj(para.ParameterType));
                }
            }
            return Activator.CreateInstance(type, test.ToArray());
        }

        public int printF(Object obj,List<Control> controls, int x, int y)
        {
            if (obj != null)
            {
                              
                Type type = obj.GetType();
                var fields = type.GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    Type type2 = Type.GetType(fieldInfo.FieldType.ToString());
                    if (type2.IsEnum)
                    {
                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20), 
                            Location = new Point(0 + x, 10 + y * 30), 
                            Text = fieldInfo.Name
                        });
                        y++;

                        ComboBox buf = new ComboBox()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20), 
                            DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList, 
                            Name = fieldInfo.Name,
                            Location = new Point(0 + x, 10 + y * 30)
                        };

                        FieldInfo[] fieldinfo2 = type2.GetFields(BindingFlags.Public | BindingFlags.Static);
                        foreach (var field2 in fieldinfo2)
                        {
                            buf.Items.Add(field2.Name.ToString());
                        }
                        controls.Add(buf);
                        y++;
                    }
                    else if ((type2.IsClass) && (type2.Name != "String"))
                    {
                        object temp = create_obj(type2);

                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Text = "Existing" + type2.ToString()
                        });
                        y++;

                        ComboBox agr = new ComboBox()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                            Location = new Point(0 + x, 10 + y * 30),
                            Name = "Existing" + type2.ToString()
                        };

                        int i = 0;
                        foreach (Object obj1 in allObj)
                        {
                            if (obj1.GetType() == type2)
                            {
                                agr.Items.Add(obj1.ToString() + (++i).ToString());
                            }
                        }

                        controls.Add(agr);
                        y++;

                        y = printF(temp, controls, x, y);
                    }
                    else
                    {
                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Text = fieldInfo.Name
                        });
                        y++;

                        controls.Add(new TextBox
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Name = fieldInfo.Name
                        });
                        y++;
                    }
                }
            }

            return y;
        }

        public Object setV(Object obj)
        {
            Type type = obj.GetType();
            var fields = type.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                Type t = Type.GetType(fieldInfo.FieldType.ToString());
                if ((t.IsClass) && (t.Name != "String"))
                {
                    Boolean flag = true;
                    foreach (Control ctrl in controls)
                    {
                        if (ctrl.Name == "Existing" + t.ToString())
                        {
                            if (ctrl.Text != "")
                            {
                                int i = 0; flag = false;
                                foreach (Object obj1 in allObj)
                                {
                                    if ((obj1.GetType() == t) && (obj1.ToString() + (++i).ToString() == ctrl.Text))
                                    {
                                        fieldInfo.SetValue(obj, obj1);
                                    }
                                }
                            }
                        }
                    }
                    if (flag)
                    {
                        object buff = create_obj(t);
                        buff = setV(buff);
                        fieldInfo.SetValue(obj, buff);
                        allObj.Add(buff);
                        comboBox2.Items.Add(buff.ToString() + allObj.Count());
                    }
                }
                else if (t.IsEnum)
                {
                    var field2 = t.GetFields();
                    
                    foreach (Control control in controls)
                    {
                        foreach (FieldInfo field in field2)
                        {
                            if (field.Name.ToString() == control.Text)
                            {
                                fieldInfo.SetValue(obj, field.GetValue(obj));
                            }
                        }
                    }
                    
                }
                else
                {
                    Object val = 0;
                    foreach (Control control in controls)
                    {
                        if (control.Name.ToString() == fieldInfo.Name.ToString())
                        {
                            val = control.Text;
                        }
                    }

                    try
                    {
                        val = Convert.ChangeType(val, fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, val);
                    }
                    catch
                    {
                        MessageBox.Show("Неправильно заполнено поле " + fieldInfo.Name);
                        return false;
                    }
                }
            }

            return obj;
        }

        public int printO(Object obj, List<Control> controls, int x, int y)
        {
            if (obj != null)
            {
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                controls.Clear();

                Type type = obj.GetType();
                var fields = type.GetFields();
                foreach (FieldInfo fieldInfo in fields)
                {
                    Type type2 = Type.GetType(fieldInfo.FieldType.ToString());
                    if ((type2.IsClass) && (type2.Name != "String"))
                    {
                        y = printO(fieldInfo.GetValue(obj), controls, x, y);
                    }
                    else
                    {
                        controls.Add(new Label()
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Text = fieldInfo.Name
                        });
                        y++;

                        controls.Add(new TextBox
                        {
                            Font = new Font("Microsoft Sans Serif", 12),
                            Size = new Size(300, 20),
                            Location = new Point(0 + x, 10 + y * 30),
                            Name = fieldInfo.Name,
                            ReadOnly = true,
                            Text = fieldInfo.GetValue(obj).ToString()
                        });
                        y++;
                    }
                }
            }

            return y;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                Object obj = create_obj(Type.GetType("Classes." + comboBox1.Text));
                obj = setV(obj);
                foreach (Control control in controls)
                {
                    this.Controls.Remove(control);
                }
                controls.Clear();
                allObj.Add(obj);
                comboBox2.Items.Add(obj.ToString() + allObj.Count());
                
            }
            else MessageBox.Show("Выберите класс");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1)
            {
                object buff = allObj[comboBox2.SelectedIndex];
                if (buff != null)
                {
                    printO(buff, controls, 200, 1);
                }
                foreach (Control control in controls)
                {
                    this.Controls.Add(control);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedIndex != -1)
            {
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                object buff = allObj[comboBox2.SelectedIndex];
                foreach (Control control in controls)
                {
                    Controls.Remove(control);
                }
                controls.Clear();
                printF(buff, controls, 200, 1);
                foreach (Control control in controls)
                {
                    Type t = buff.GetType();
                    var fields = t.GetFields();
                    foreach (FieldInfo fieldInfo in fields)
                    {
                        if (fieldInfo.Name.ToString() == control.Name.ToString())
                        {
                            control.Text = fieldInfo.GetValue(buff).ToString();
                        }
                    }
                }
                foreach (Control control in controls)
                {
                    this.Controls.Add(control);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            foreach (Control control in controls)
            {
                Controls.Remove(control);
            }
            controls.Clear();
            object buff = allObj[comboBox2.SelectedIndex];
            comboBox2.Items.Remove(comboBox2.SelectedItem);
            comboBox2.SelectedIndex = -1;
            comboBox2.Text = "";
            allObj.Remove(buff);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (comboBox1.Text != "")
            {
                Object obj = create_obj(Type.GetType("Classes." + comboBox1.Text));
                printF(obj, this.controls, 200, 1);
            }
            foreach (Control control in controls)
            {
                this.Controls.Add(control);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            object buff = allObj[comboBox2.SelectedIndex];
            allObj[comboBox2.SelectedIndex] = setV(buff);
            foreach (Control control in controls)
            {
                this.Controls.Remove(control);
            }
            controls.Clear();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex != -1)
            {
                ISer meth = (ISer)Activator.CreateInstance(Type.GetType("Serial." + comboBox3.Text));

                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                string filename = saveFileDialog1.FileName;
                meth.Serialize(allObj, filename);
                pm.Plugins[comboBox4.SelectedIndex].Compress(filename);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            
            if (comboBox3.SelectedIndex != -1)
            {
                allObj.Clear();
                ISer meth = (ISer)Activator.CreateInstance(Type.GetType("Serial." + comboBox3.Text));

                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                string filename = openFileDialog1.FileName;
                Regex regex = new Regex(@"(\.[\w]+){0,1}$");
                MatchCollection matches = regex.Matches(filename);
                string exten = matches[0].Groups[0].Value;
                if (matches.Count != 1)
                {
                    bool flag = false;
                    foreach (var plugin in pm.Plugins)
                    {
                        if (exten == plugin.Extension)
                        {
                            filename = plugin.Decompress(filename);
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        allObj = meth.Deserialize(filename);
                        comboBox2.Items.Clear();
                        int c = 0;
                        foreach (Object obj in allObj)
                        {
                            comboBox2.Items.Add(obj.ToString() + ++c);
                        }
                        File.Delete(filename);
                    }
                    else
                    {
                        MessageBox.Show("Плагин не загружен");
                    }
                }
                else
                {
                    MessageBox.Show("Не указано расширение файла");
                }
            }
        }
    }
}

namespace Serial
{
    using Classes;
    using OOP2;
    interface ISer
    {
        void Serialize(List<Object> Objects, string FileName);
        List<Object> Deserialize(string FileName);
    }
    class CustomSer : ISer
    {
        public static Object create_obj(Type type)
        {
            ConstructorInfo[] cons = type.GetConstructors();
            ParameterInfo[] pars = cons[0].GetParameters();
            List<Object> test = new List<Object>();
            if (pars.Length == 0)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                foreach (var para in pars)
                {
                    test.Add(create_obj(para.ParameterType));
                }
            }
            return Activator.CreateInstance(type, test.ToArray());
        }
        public void getValue(Object obj, List<string> val)
        {
            Type t = obj.GetType();
            var fields = t.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                Type type2 = Type.GetType(fieldInfo.FieldType.ToString());
                if ((type2.IsClass) && (type2.Name != "String"))
                {
                    getValue(fieldInfo.GetValue(obj), val);
                }
                else
                {
                    val.Add(fieldInfo.GetValue(obj).ToString() + "|");
                }
            }
        }

        public void Serialize(List<Object> Objects, string FileName)
        {
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                StreamWriter writer = new StreamWriter(fs);
                try
                {
                    foreach (Object obj in Objects)
                    {
                        Type t = obj.GetType();
                        writer.Write(t.ToString());
                        writer.WriteLine();

                        List<string> values = new List<string>();
                        getValue(obj, values);

                        foreach (string s in values)
                        {
                            writer.Write(s);
                        }

                        writer.WriteLine();
                    }
                    writer.Close();
                    fs.Close();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }
        public Object setVal(Object obj, string[] vals, int i)
        {
            Type t = obj.GetType();
            var fields = t.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                Type t2 = Type.GetType(fieldInfo.FieldType.ToString());
                if ((t2.IsClass) && (t2.Name != "String"))
                {
                    object buff = create_obj(t2);
                    buff = setVal(buff, vals, i);
                    Boolean flag = true;
                    foreach (Object ob in all)
                    {
                        if (ob.GetType() == buff.GetType())
                        {
                            var f1 = ob.GetType().GetFields();
                            foreach (FieldInfo f in f1)
                            {
                                var a = f.GetValue(ob);
                                var b = f.GetValue(buff);
                                if (a.Equals(b))
                                {
                                    flag = false;
                                }
                            }
                        }

                    }
                    if (flag)
                    {
                        all.Add(buff);
                    }
                    fieldInfo.SetValue(obj, buff);
                }
                else if (t2.IsEnum)
                {
                    var field2 = t2.GetFields();
                    Object val = 0;
                    val = vals[i];
                    i++;
                    foreach (FieldInfo field in field2)
                    {
                        if (field.Name.ToString() == val.ToString())
                        {
                            fieldInfo.SetValue(obj, field.GetValue(obj));
                        }
                    }
                }
                else
                {
                    Object val = 0;
                    val = vals[i];
                    i++;
                    try
                    {
                        val = Convert.ChangeType(val, fieldInfo.FieldType);
                        fieldInfo.SetValue(obj, val);
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return obj;
        }
        List<Object> all = new List<Object>();
        public List<Object> Deserialize(string FileName)
        {
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                StreamReader reader = new StreamReader(fs);
                string line;
                try
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        Object obj = create_obj(Type.GetType(line));
                        line = reader.ReadLine();
                        string[] vals = line.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
                        setVal(obj, vals, 0);
                        all.Add(obj);
                    }
                    reader.Close();
                    fs.Close();
                    return all;
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                List<Object> empty = new List<Object>();
                return empty;
            }
        }
    }

    class BinSer : ISer
    {
        public void Serialize(List<Object> Objects, string FileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                try
                {
                    formatter.Serialize(fs, Objects);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Oooops :(\n {0}", e.ToString());
                }
            }
        }
        public List<Object> Deserialize(string FileName)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                try
                {
                    return (List<Object>)formatter.Deserialize(fs);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Oooops :(\n {0}", e.ToString());
                }
                List<Object> empty = new List<Object>();
                return empty;
            }
        }
    }
    class XMLSer : ISer
    {
        public void Serialize(List<Object> Objects, string FileName)
        {
            Type[] classes = new Type[9];
            classes[0] = typeof(Ammo);
            classes[1] = typeof(ShotgunAmmo);
            classes[2] = typeof(Weapon);
            classes[3] = typeof(Melee);
            classes[4] = typeof(Special);
            classes[5] = typeof(Firearm);
            classes[6] = typeof(Handgun);
            classes[7] = typeof(Rifle);
            classes[8] = typeof(Shotgun);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Object>), classes);
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                try
                {
                    xmlSerializer.Serialize(fs, Objects);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Oooops :(\n {0}", e.ToString());
                }
            }
        }
        public List<Object> Deserialize(string FileName)
        {
            Type[] classes = new Type[9];
            classes[0] = typeof(Ammo);
            classes[1] = typeof(ShotgunAmmo);
            classes[2] = typeof(Weapon);
            classes[3] = typeof(Melee);
            classes[4] = typeof(Special);
            classes[5] = typeof(Firearm);
            classes[6] = typeof(Handgun);
            classes[7] = typeof(Rifle);
            classes[8] = typeof(Shotgun);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Object>), classes);
            using (FileStream fs = new FileStream(FileName, FileMode.OpenOrCreate))
            {
                try
                {
                    return (List<Object>)xmlSerializer.Deserialize(fs);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Oooops :(\n {0}", e.ToString());
                }
                List<Object> empty = new List<Object>();
                return empty;
            }
        }
    }
}

namespace Classes
{
    [Serializable]
    public class Ammo
    {
        public int speed;
        public int range;

        public enum T1
        {
            LRN,
            WC,
            FMJ,
        }

        public T1 type;
    }

    [Serializable]
    public class ShotgunAmmo
    {
        public int speed;
        public int range;

        public enum T2
        {
            Buckshot,
            Slug,
        }
        public T2 type;
    }

    [Serializable]
    public class Weapon
    {
        public int lethality;
        public int weight;
        public int model;
    }

    [Serializable]
    public class Melee : Weapon
    {
        public int length;

        public enum T3
        {
            destruction,
            erosion,
        }
        public T3 impact;
    }

    [Serializable]
    public class Special : Weapon
    {
        public int range;
        public enum T4
        {
            SniperRifle,
            FusionRifle,
            Sidearm,
        }
        public T4 type;
    }

    [Serializable]
    public class Firearm : Weapon
    {
        public int rateOfFire;
        public int magCapacity;

        public enum T5
        {
            GAP,
            M24,
        }

        public T5 barrel;
    }

    [Serializable]
    public class Handgun : Firearm
    {
        public Ammo ammo;

        public enum C1
        {
            mm4,
            mm5,
            mm6,
            mm7,
        }

        public enum T6
        {
            Colt,
            CZ,
            FN,
        }

        public C1 caliber;
        public T6 type;
    }

    [Serializable]
    public class Rifle : Firearm
    {
        public Ammo ammo;

        public enum T7
        {
            Hunting,
            Fishing,
            Optics,
        }
        public T7 type;
    }

    [Serializable]
    public class Shotgun : Firearm
    {
        public ShotgunAmmo ammo;

        public enum C2
        {
            Gauge1,
            Gauge7,
            Gauge24,
        }

        public enum T8
        {
            Breakaction,
            Pumpaction,
            Semiautomatic,
        }
        public C2 caliber;
        public T8 type;
    }
}
