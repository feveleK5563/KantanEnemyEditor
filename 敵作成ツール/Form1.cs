using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 敵作成ツール
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public struct KM_Vector3 //適当に作ったK_Math::Vector3
        {
            public float x;
            public float y;
            public float z;

            public void Set(decimal setx, decimal sety, decimal setz)
            {
                x = (float)setx;
                y = (float)sety;
                z = (float)setz;
            }
            public string Output()
            {
                return (x + " " + y + " " + z);
            }
        }

        public struct KM_Box2D //こっちはK_Math::Box2D
        {
            public int x;
            public int y;
            public int w;
            public int h;

            public void Set(decimal setx, decimal sety, decimal setw, decimal seth)
            {
                x = (int)setx;
                y = (int)sety;
                w = (int)setw;
                h = (int)seth;
            }
            public string Output()
            {
                return (x + " " + y + " " +
                        w + " " + h);
            }
        }

        public class EnemyParamater //パラメーター情報
        {
            public string texturePath;
            public string textureName;
            public int maxLife;
            public int hitDamage;
            public float moveSpeed;
            public float jumpPower;
            public int isUseGravity;
        }
        public EnemyParamater enemyParamater = new EnemyParamater();

        public class EnemyCollision //コリジョンサイズ情報
        {
            public KM_Vector3 baseShapeSize;

            public KM_Vector3 dcPos;
            public KM_Vector3 dcShapeSize;

            public KM_Vector3 visibillityPos;
            public KM_Vector3 visibillityShapeSize;

            public KM_Vector3 attackTransPos;
            public KM_Vector3 attackTransShapeSize;
        }
        public EnemyCollision enemyCollision = new EnemyCollision();

        public class EnemyMovePattern //動作パターン一個単位
        {
            public int totalMoveNum;
            public class EnemyMove
            {
                public int moveId;
                public int behaviorId;
                public int durationTime;
                public KM_Box2D animSrc;
                public float basisRenderPosX;
                public float basisRenderPosY;
                public int animNum;
                public float waitTime;
                public int isRoop;
            }
            public List<EnemyMove> em = new List<EnemyMove>();
        }

        public class EnemyMoveSetList //動作パターンまとめ
        {
            public int totalPatternNum = 0;
            public class EnemyMoveSet
            {
                public EnemyMovePattern emp = new EnemyMovePattern();
                public List<int> transitionId = new List<int>();
            }
            public List<EnemyMoveSet> ems = new List<EnemyMoveSet>();
        }
        public EnemyMoveSetList enemyMSList = new EnemyMoveSetList();
        public int nowSettingMove;
        public int nowSettingMovePattern;
        public bool readFile;

        private void Form1_Load(object sender, EventArgs e)
        {
            nowSettingMove = 0;
            nowSettingMovePattern = 0;
            readFile = false;
            Message.Text = "パラメータとコリジョンサイズ、位置を設定してください";

            //-------------------------
            //動作ID
            MoveID.Items.Add("0：何もしない");
            MoveID.Items.Add("1：向いている方向に移動する");
            MoveID.Items.Add("2：ジャンプする");
            MoveID.Items.Add("3：前方に攻撃用コリジョンを生成する");

            //-------------------------
            //動作遷移ID
            TransitionID.Items.Add("0：遷移しない");
            TransitionID.Items.Add("1：動作パターンが一巡したとき");
            TransitionID.Items.Add("2：視界内にプレイヤーが入っているとき");


            MoveID.SelectedIndex = 0;
            TransitionID.SelectedIndex = 0;
        }

        private void DicisionParamaterAndCollisionSize_Click(object sender, EventArgs e)
        {
            //入力忘れを認識
            if ((TexturePath.Text.Length <= 0) ||
                (TextureName.Text.Length <= 0))
            {
                MessageBox.Show("テキストボックスに入力もできないの？\nそんなんじゃ甘いよ（嘲笑）",
                                "まだ肝心なとこ入力し忘れてるゾ",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Hand);
                return;
            }

            //確認
            DialogResult result = MessageBox.Show("パラメータとコリジョン設定を確定しますか？\n（確定すると以後変更できません）",
                                                    "確認",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Exclamation);
            //「いいえ」が選択された時はキャンセル
            if (result == DialogResult.No)
            {
                return;
            }


            //各種パラメータを設定
            enemyParamater.texturePath = TexturePath.Text;
            enemyParamater.textureName = TextureName.Text;
            enemyParamater.maxLife = (int)MaxLife.Value;
            enemyParamater.hitDamage = (int)HitDamage.Value;
            enemyParamater.moveSpeed = (float)MoveSpeed.Value;
            enemyParamater.jumpPower = (float)JumpPower.Value;
            enemyParamater.isUseGravity = IsUseGrabity.Checked ? 1 : 0;

            //動作パターンの数をここで設定
            PatternOrder.Text = "0";
            MoveOrder.Text = "0";
            for (int i = 0; i < (int)MovePatternNum.Value; ++i)
            {
                TransitionList.Items.Add(i.ToString() + "： 0 " + true.ToString());
                SetMovePatternID.Items.Add("動作" + i.ToString());
                if (i < enemyMSList.totalPatternNum)
                {
                    for (int j = 0; j < (int)MovePatternNum.Value - enemyMSList.totalPatternNum; ++j)
                    {
                        enemyMSList.ems[i].transitionId.Add(new int());
                        enemyMSList.ems[i].transitionId[j] = 0;
                    }
                    continue;
                }

                enemyMSList.ems.Add(new EnemyMoveSetList.EnemyMoveSet());
                for (int j = 0; j < (int)MovePatternNum.Value; ++j)
                {
                    enemyMSList.ems[i].transitionId.Add(new int());
                    enemyMSList.ems[i].transitionId[j] = 0;
                }
            }
            enemyMSList.totalPatternNum = (int)MovePatternNum.Value;
            SetMovePatternID.SelectedIndex = 0;

            if (readFile == true)
            {
                SettingMoveSet(0);
            }

            //コリジョンの形状を設定
            enemyCollision.baseShapeSize.
                Set(BaseW.Value, BaseH.Value, BaseD.Value);
            enemyCollision.dcShapeSize.
                Set(DamageCameraW.Value, DamageCameraH.Value, DamageCameraD.Value);
            enemyCollision.visibillityShapeSize.
                Set(VisibillityW.Value, VisibillityH.Value, VisibillityD.Value);
            enemyCollision.attackTransShapeSize.
                Set(AttackTransW.Value, AttackTransH.Value, AttackTransD.Value);

            enemyCollision.dcPos.
                Set(DamageCameraPosX.Value, DamageCameraPosY.Value, DamageCameraPosZ.Value);
            enemyCollision.visibillityPos.
                Set(VisibillityPosX.Value, VisibillityPosY.Value, VisibillityPosZ.Value);
            enemyCollision.attackTransPos.
                Set(AttackTransPosX.Value, AttackTransPosY.Value, AttackTransPosZ.Value);

            //以後パラメーターとコリジョンの変更は不可にする
            //ファイル読み込みもできなくする
            ParameterCollisionPanel.Enabled = false;
            ReadFileName.Text = "";
            ReadFilePanel.Enabled = false;
            //動作設定が可能にする
            MovePatternPanel.Enabled = true;
            CreateEnemy.Enabled = true;

            MoveList.SelectedIndex = 0;
            TransitionList.SelectedIndex = 0;

            Message.Text = "動作パターンを設定してください";
        }

        private void CreateOneMove_Click(object sender, EventArgs e)
        {
            //動作の設定
            EnemyMovePattern.EnemyMove setem = new EnemyMovePattern.EnemyMove();
            setem.moveId = MoveID.SelectedIndex;
            setem.behaviorId = IsGetBehavior.Checked == true ? setem.moveId : 0;
            setem.durationTime = (int)DurationTime.Value;
            setem.animSrc.Set(BoxX.Value, BoxY.Value, BoxW.Value, BoxH.Value);
            setem.basisRenderPosX = (float)BasisRenderX.Value;
            setem.basisRenderPosY = (float)BasisRenderY.Value;
            setem.animNum = (int)AnimationNum.Value;
            setem.waitTime = (float)WaitTime.Value;
            setem.isRoop = IsRoop.Checked ? 1 : 0;

            int add = MoveList.SelectedIndex;
            //ここに追加を選択していない時
            if (add != MoveList.Items.Count - 1)
            {
                enemyMSList.ems[nowSettingMovePattern].emp.em.RemoveAt(add);
                MoveList.Items.RemoveAt(add);
                enemyMSList.ems[nowSettingMovePattern].emp.em.Insert(add, setem);
                MoveList.Items.Insert(add, setem.moveId.ToString() + " " + IsGetBehavior.Checked + " " + setem.durationTime.ToString());
                MoveList.SelectedIndex = add;
            }
            else
            {
                ++enemyMSList.ems[nowSettingMovePattern].emp.totalMoveNum;
                enemyMSList.ems[nowSettingMovePattern].emp.em.Insert(add, setem);
                MoveList.Items.Insert(add, setem.moveId.ToString() + " " + IsGetBehavior.Checked + " " + setem.durationTime.ToString());
            }

            IsRoop.Checked = false;

            ++nowSettingMove;
            MoveOrder.Text = nowSettingMove.ToString();
        }

        private void ResetMove_Click(object sender, EventArgs e)
        {
            nowSettingMove = 0;
            MoveOrder.Text = nowSettingMove.ToString();
            enemyMSList.ems[nowSettingMovePattern].emp.em.Clear();
            enemyMSList.ems[nowSettingMovePattern].emp.em.Capacity = 0;
            enemyMSList.ems[nowSettingMovePattern].emp.totalMoveNum = 0;
            IsRoop.Checked = false;

            MoveList.Items.Clear();
            MoveList.Items.Add("ここに追加する");
            MoveList.SelectedIndex = 0;
        }

        private void SetTransition_Click(object sender, EventArgs e)
        {
            enemyMSList.ems[nowSettingMovePattern].transitionId[TransitionList.SelectedIndex] = TransitionID.SelectedIndex;
            if (ReturnFalse.Checked)
            {
                enemyMSList.ems[nowSettingMovePattern].transitionId[TransitionList.SelectedIndex] *= -1;
            }

            int add = TransitionList.SelectedIndex;
            TransitionList.Items.RemoveAt(add);
            TransitionList.Items.Insert(add, add.ToString() + "： " + TransitionID.SelectedIndex.ToString() + " " + (!ReturnFalse.Checked).ToString());

            TransitionList.SelectedIndex = add;
        }

        private void SettingMoveSet(int setNum)
        {
            nowSettingMovePattern = setNum;

            PatternOrder.Text = setNum.ToString();

            //動作内容をリストボックスに表示
            MoveList.Items.Clear();
            for (int i = 0; i < enemyMSList.ems[nowSettingMovePattern].emp.em.Count; ++i)
            {
                bool be = enemyMSList.ems[nowSettingMovePattern].emp.em[i].behaviorId != 0 ? true : false;
                MoveList.Items.Add(enemyMSList.ems[nowSettingMovePattern].emp.em[i].moveId.ToString() + " " +
                                   be.ToString() + " " +
                                   enemyMSList.ems[nowSettingMovePattern].emp.em[i].durationTime.ToString());
            }
            MoveOrder.Text = enemyMSList.ems[nowSettingMovePattern].emp.em.Count.ToString();
            nowSettingMove = enemyMSList.ems[nowSettingMovePattern].emp.em.Count;
            MoveList.Items.Add("ここに追加する");
            MoveList.SelectedIndex = MoveList.Items.Count - 1;

            //遷移内容をリストボックスに表示
            TransitionList.Items.Clear();
            for (int i = 0; i < enemyMSList.ems[nowSettingMovePattern].transitionId.Count; ++i)
            {
                TransitionList.Items.Add(i.ToString() + "： " + Math.Abs(enemyMSList.ems[nowSettingMovePattern].transitionId[i]).ToString());
                if (enemyMSList.ems[nowSettingMovePattern].transitionId[i] >= 0)
                {
                    TransitionList.Items[TransitionList.Items.Count - 1] += " " + true.ToString();
                }
                else
                {
                    TransitionList.Items[TransitionList.Items.Count - 1] += " " + false.ToString();
                }
            }
            TransitionList.SelectedIndex = TransitionList.Items.Count - 1;
        }

        private void DicisionMovePattern_Click(object sender, EventArgs e)
        {
            if (nowSettingMovePattern == SetMovePatternID.SelectedIndex)
            {
                return;
            }

            SettingMoveSet(SetMovePatternID.SelectedIndex);
        }

        private void AllReset_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("ここまで作成したEnemyの情報をリセットしますがよろしいですか？\n（強制再起動します）",
                                                  "※注意※",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Exclamation);
            //「いいえ」が選択された時はキャンセル
            if (result == DialogResult.No)
            {
                return;
            }

            result = MessageBox.Show("ほんとぉ？",
                                     "※最終確認※",
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Exclamation);
            //「いいえ」が選択された時はキャンセル
            if (result == DialogResult.No)
            {
                return;
            }

            Application.Restart();
        }

        private void CreateEnemy_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < enemyMSList.ems.Count; ++i)
            {
                if (enemyMSList.ems[i].emp.em.Count <= 0)
                {
                    MessageBox.Show("やっぱり" + i.ToString() + "番の動作入力し忘れてるじゃないか（憤怒）\n君じゃ話にならないから責任者呼んできて",
                                    "は？（威圧）",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Hand);

                    return;
                }
            }

            if (OutputFileName.Text.Length <= 0)
            {

                MessageBox.Show("ファイル出力するっつうのにファイル名書いてねぇっておかしいだろそれよぉ！？\n違うか！？オイ！",
                                "あのさぁ…（棒読み）",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Hand);

                return;
            }

            DialogResult result = MessageBox.Show("これまで入力した情報をsrcファイルに出力します\n（既に同名のファイルが存在している場合上書きされます）",
                                                  "確認",
                                                  MessageBoxButtons.YesNo,
                                                  MessageBoxIcon.Exclamation);
            //「いいえ」が選択された時はキャンセル
            if (result == DialogResult.No)
            {
                return;
            }

            System.IO.FileStream fs = System.IO.File.Create("data/" + OutputFileName.Text + ".txt");
            System.IO.StreamWriter sw = new System.IO.StreamWriter(fs);

            //パラメータとコリジョン
            sw.WriteLine(enemyParamater.texturePath + " ");
            sw.WriteLine(enemyParamater.textureName + " ");
            sw.WriteLine(enemyParamater.maxLife + " " + enemyParamater.hitDamage + " " + enemyParamater.moveSpeed + " " +
                         enemyParamater.jumpPower + " " + enemyParamater.isUseGravity + " ");

            sw.WriteLine(enemyCollision.baseShapeSize.Output() + " ");
            sw.WriteLine(enemyCollision.dcPos.Output() + " ");
            sw.WriteLine(enemyCollision.dcShapeSize.Output() + " ");
            sw.WriteLine(enemyCollision.visibillityPos.Output() + " ");
            sw.WriteLine(enemyCollision.visibillityShapeSize.Output() + " ");
            sw.WriteLine(enemyCollision.attackTransPos.Output() + " ");
            sw.WriteLine(enemyCollision.attackTransShapeSize.Output() + " ");

            //動作パターン
            sw.WriteLine(enemyMSList.totalPatternNum + " ");
            for (int i = 0; i < enemyMSList.totalPatternNum; ++i)
            {
                sw.WriteLine(enemyMSList.ems[i].emp.totalMoveNum + " ");
                for (int j = 0; j < enemyMSList.ems[i].emp.totalMoveNum; ++j)
                {
                    sw.WriteLine(enemyMSList.ems[i].emp.em[j].moveId + " " +
                                 enemyMSList.ems[i].emp.em[j].behaviorId + " " +
                                 enemyMSList.ems[i].emp.em[j].durationTime + " ");
                    sw.WriteLine(enemyMSList.ems[i].emp.em[j].animSrc.Output() + " " +
                                 enemyMSList.ems[i].emp.em[j].basisRenderPosX + " " +
                                 enemyMSList.ems[i].emp.em[j].basisRenderPosY + " " +
                                 enemyMSList.ems[i].emp.em[j].animNum + " " +
                                 enemyMSList.ems[i].emp.em[j].waitTime + " " +
                                 enemyMSList.ems[i].emp.em[j].isRoop + " ");
                }
                
                for (int k = 0; k < enemyMSList.totalPatternNum; ++k)
                {
                    sw.Write(enemyMSList.ems[i].transitionId[k] + " ");
                }
                sw.WriteLine("");
            }

            sw.Close();
            fs.Close();

            MessageBox.Show(OutputFileName.Text + ".txtの出力が完了しました",
                            "完了",
                            MessageBoxButtons.OK);
        }

        private void ReadFile_Click(object sender, EventArgs e)
        {
            if (ReadFileName.Text.Length <= 0)
            {
                MessageBox.Show("先輩！何でファイル名書いてないんすか！\nやめてくださいよ本当に！",
                                "ああああああ！！テメェェェ！！何してんだよァァァ！！",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Hand);

                return;
            }

            enemyMSList.ems.Clear();

            System.IO.StreamReader sr = null;
            try
            {
                sr = new System.IO.StreamReader("readData/" + ReadFileName.Text + ".txt");

                string[] txt = sr.ReadToEnd().Split(' ');
                int tmp = 0;

                TexturePath.Text = txt[tmp++];
                TextureName.Text = txt[tmp++];
                MaxLife.Value = decimal.Parse(txt[tmp++]);
                HitDamage.Value = decimal.Parse(txt[tmp++]);
                MoveSpeed.Value = decimal.Parse(txt[tmp++]);
                JumpPower.Value = decimal.Parse(txt[tmp++]);
                IsUseGrabity.Checked = int.Parse(txt[tmp++]) == 1;
                
                BaseW.Value = decimal.Parse(txt[tmp++]);
                BaseH.Value = decimal.Parse(txt[tmp++]);
                BaseD.Value = decimal.Parse(txt[tmp++]);

                DamageCameraPosX.Value = decimal.Parse(txt[tmp++]);
                DamageCameraPosY.Value = decimal.Parse(txt[tmp++]);
                DamageCameraPosZ.Value = decimal.Parse(txt[tmp++]);
                DamageCameraW.Value = decimal.Parse(txt[tmp++]);
                DamageCameraH.Value = decimal.Parse(txt[tmp++]);
                DamageCameraD.Value = decimal.Parse(txt[tmp++]);
                VisibillityPosX.Value = decimal.Parse(txt[tmp++]);
                VisibillityPosY.Value = decimal.Parse(txt[tmp++]);
                VisibillityPosZ.Value = decimal.Parse(txt[tmp++]);
                VisibillityW.Value = decimal.Parse(txt[tmp++]);
                VisibillityH.Value = decimal.Parse(txt[tmp++]);
                VisibillityD.Value = decimal.Parse(txt[tmp++]);
                AttackTransPosX.Value = decimal.Parse(txt[tmp++]);
                AttackTransPosY.Value = decimal.Parse(txt[tmp++]);
                AttackTransPosZ.Value = decimal.Parse(txt[tmp++]);
                AttackTransW.Value = decimal.Parse(txt[tmp++]);
                AttackTransH.Value = decimal.Parse(txt[tmp++]);
                AttackTransD.Value = decimal.Parse(txt[tmp++]);

                MovePatternNum.Value = decimal.Parse(txt[tmp++]);
                enemyMSList.totalPatternNum = (int)MovePatternNum.Value;
                for (int i = 0; i < enemyMSList.totalPatternNum; ++i)
                {
                    enemyMSList.ems.Add(new EnemyMoveSetList.EnemyMoveSet());

                    enemyMSList.ems[i].emp.totalMoveNum = int.Parse(txt[tmp++]);

                    for (int j = 0; j < enemyMSList.ems[i].emp.totalMoveNum; ++j)
                    {
                        EnemyMovePattern.EnemyMove setem = new EnemyMovePattern.EnemyMove();
                        setem.moveId = int.Parse(txt[tmp++]);
                        setem.behaviorId = int.Parse(txt[tmp++]);
                        setem.durationTime = int.Parse(txt[tmp++]);
                        setem.animSrc.Set(int.Parse(txt[tmp++]), int.Parse(txt[tmp++]), int.Parse(txt[tmp++]), int.Parse(txt[tmp++]));
                        setem.basisRenderPosX = float.Parse(txt[tmp++]);
                        setem.basisRenderPosY = float.Parse(txt[tmp++]);
                        setem.animNum = int.Parse(txt[tmp++]);
                        setem.waitTime = int.Parse(txt[tmp++]);
                        setem.isRoop = int.Parse(txt[tmp++]) == 1 ? 1 : 0;

                        enemyMSList.ems[i].emp.em.Add(setem);
                    }

                    for (int k = 0; k < enemyMSList.totalPatternNum; ++k)
                    {
                        enemyMSList.ems[i].transitionId.Add(int.Parse(txt[tmp++]));
                    }
                }

                MovePatternNum.Minimum = enemyMSList.totalPatternNum;
                OutputFileName.Text = ReadFileName.Text;
                readFile = true;

                MessageBox.Show("読み込みが完了しました",
                                "完了",
                                MessageBoxButtons.OK);
            }
            catch
            {
                MessageBox.Show("指定したファイルが無いか、\n正常なファイルでない可能性が微粒子レベルで存在している…？",
                                "データ壊れちゃ～↑う",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Hand);

                return;
            }

            return;
        }

        private void MoveList_SelectedIndexChanged(object sender, EventArgs e)
        {
            int add = MoveList.SelectedIndex;
            if (add == MoveList.Items.Count - 1 || add < 0)
            {
                CreateOneMove.Text = "動作を追加";
                return;
            }

            MoveID.SelectedIndex = enemyMSList.ems[nowSettingMovePattern].emp.em[add].moveId;
            IsGetBehavior.Checked = enemyMSList.ems[nowSettingMovePattern].emp.em[add].behaviorId != 0 ? true : false;
            DurationTime.Value = enemyMSList.ems[nowSettingMovePattern].emp.em[add].durationTime;
            BoxX.Value = enemyMSList.ems[nowSettingMovePattern].emp.em[add].animSrc.x;
            BoxY.Value = enemyMSList.ems[nowSettingMovePattern].emp.em[add].animSrc.y;
            BoxW.Value = enemyMSList.ems[nowSettingMovePattern].emp.em[add].animSrc.w;
            BoxH.Value = enemyMSList.ems[nowSettingMovePattern].emp.em[add].animSrc.h;
            BasisRenderX.Value = (decimal)enemyMSList.ems[nowSettingMovePattern].emp.em[add].basisRenderPosX;
            BasisRenderY.Value = (decimal)enemyMSList.ems[nowSettingMovePattern].emp.em[add].basisRenderPosY;
            AnimationNum.Value = enemyMSList.ems[nowSettingMovePattern].emp.em[add].animNum;
            WaitTime.Value = (decimal)enemyMSList.ems[nowSettingMovePattern].emp.em[add].waitTime;
            IsRoop.Checked = enemyMSList.ems[nowSettingMovePattern].emp.em[add].isRoop == 1;

            CreateOneMove.Text = "動作を更新";
        }
    }
}
