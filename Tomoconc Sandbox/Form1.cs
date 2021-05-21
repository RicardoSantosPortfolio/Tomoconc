using Kitware.VTK;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Tomoconc_Sandbox
{
    public partial class Form1 : Form
    {

        [DllImport("cudaMarchingCubes.dll", CallingConvention = CallingConvention.Cdecl)]

        public static extern IntPtr cudaMarchingCubes(IntPtr entrada, Int32 width, Int32 height, Int32 threshold);

        private int[] vetorImagem;
        private int[] vetorImagemProcessada;
        private Bitmap toShow;

        //Variáveis do VTK
        vtkPoints pontosVTK;
        vtkActor atorVTK;
        vtkRenderer rendererVTK;
        vtkRenderWindow windowVTK;
        vtkActor actorVTK;

        public Form1()
        {
            InitializeComponent();
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
           // Console.WriteLine();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                toShow = new Bitmap(openFileDialog1.FileName);

                vetorImagem = new int[toShow.Height * toShow.Width];
                vetorImagemProcessada = new int[toShow.Height * toShow.Width];

                //======================================================================================================
                //Como me livrar disto aqui?????
                //                             \/     Código serial lento! Odeio você!     \/
                for (int i = 0; i < toShow.Width; i++)
                    for (int j = 0; j < toShow.Height; j++)
                        vetorImagem[j + (i * toShow.Width)] = (toShow.GetPixel(i, j).A << 24) | (toShow.GetPixel(i, j).R << 16) | (toShow.GetPixel(i, j).G << 8) | toShow.GetPixel(i, j).B;
                //                             /\     Código serial lento! Odeio você!     /\
                //======================================================================================================

                int size = Marshal.SizeOf(vetorImagem[0]) * vetorImagem.Length;

                IntPtr ptrImagemAProcesar = Marshal.AllocHGlobal(size);

                Marshal.Copy(vetorImagem, 0, ptrImagemAProcesar, vetorImagem.Length);

                int size2 = Marshal.SizeOf(new Int32()) * toShow.Width * toShow.Height;

                IntPtr ptrCudaImagem = Marshal.AllocHGlobal(size2);
                
                ptrCudaImagem = cudaMarchingCubes(ptrImagemAProcesar, toShow.Width, toShow.Height, trackBar1.Value);

                Marshal.FreeHGlobal(ptrImagemAProcesar);

                Marshal.Copy(ptrCudaImagem, vetorImagemProcessada, 0, toShow.Height * toShow.Width);

                //======================================================================================================
                //Como me livrar disto aqui?????
                //                             \/     Código serial lento! Odeio você!     \/
                for (int i = 0; i < toShow.Width; i++)
                    for (int j = 0; j < toShow.Height; j++)
                        toShow.SetPixel(j, i, (Color.FromArgb(vetorImagemProcessada[j + (i * toShow.Width)])));
                //                             /\     Código serial lento! Odeio você!     /\
                //======================================================================================================


                pictureBox1.Image = toShow;

                this.renderScene();
            }
        }

        private void renderWindowControl1_Load(object sender, EventArgs e)
        {
            //Inicializando componentes não-VTK
            label2.Text = "" + trackBar1.Value;


            //Início do código VTK
            pontosVTK = vtkPoints.New();

            //Criando os componentes de visualização
            rendererVTK = renderWindowControl1.RenderWindow.GetRenderers().GetFirstRenderer();
            windowVTK = renderWindowControl1.RenderWindow;

            //rendererVTK.SetBackground(1, 0, 0);
            //renderWindowControl1.AddTestActors = true;

            this.renderScene();
        }

        private void renderScene()
        {
            vtkCamera camera;

            if (toShow != null)
            {
                this.geometryPoints();

                vtkPolyData polydata = vtkPolyData.New();
                polydata.SetPoints(pontosVTK);

                vtkDelaunay2D del = vtkDelaunay2D.New();
                del.AddInput(polydata);

                vtkPolyDataMapper mapMesh = vtkPolyDataMapper.New();
                mapMesh.SetInput(del.GetOutput());

                vtkActor meshActor = vtkActor.New();
                meshActor.SetMapper(mapMesh);
                meshActor.GetProperty().SetColor(0.5d, 0.5d, 0d);

                windowVTK.AddRenderer(rendererVTK);
                rendererVTK.AddActor(meshActor);

                windowVTK.Render();
                rendererVTK.Render();

                meshActor.Render(rendererVTK, mapMesh);
                camera = rendererVTK.GetActiveCamera();
                camera.Zoom(1.0d);

                rendererVTK.ResetCamera();

            }
        }

        private void geometryPoints()
        {
            int pointId = 0;
            //Só colocamos nos pontos da janela VTK caso o pixel não seja preto.
            for (int i = 0; i < toShow.Height; i++)
                for (int j = 0; j < toShow.Width; j++)
                    if (toShow.GetPixel(j, i).ToArgb() != Color.Black.ToArgb() )
                    {

                        pontosVTK.InsertPoint(pointId, j, i, 0);
                        pointId++;
                        //pontosVTK.InsertNextPoint(i, j, 0);
                    }

            Console.WriteLine("" + pontosVTK.GetNumberOfPoints());
            return;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            label2.Text = "" + trackBar1.Value;

            if (toShow != null)
            {
                int size, size2;

                size = Marshal.SizeOf(vetorImagem[0]) * vetorImagem.Length;

                IntPtr ptrImagemAProcesar = Marshal.AllocHGlobal(size);

                Marshal.Copy(vetorImagem, 0, ptrImagemAProcesar, vetorImagem.Length);

                size2 = Marshal.SizeOf(new Int32()) * toShow.Width * toShow.Height;

                IntPtr ptrCudaImagem = Marshal.AllocHGlobal(size2);

                ptrCudaImagem = cudaMarchingCubes(ptrImagemAProcesar, toShow.Width, toShow.Height, trackBar1.Value);

                Marshal.Copy(ptrCudaImagem, vetorImagemProcessada, 0, toShow.Height * toShow.Width);

                //======================================================================================================
                //Como me livrar disto aqui?????
                //                             \/     Código serial lento! Odeio você!     \/
                for (int i = 0; i < toShow.Width; i++)
                    for (int j = 0; j < toShow.Height; j++)
                        toShow.SetPixel(j, i, (Color.FromArgb(vetorImagemProcessada[j + (i * toShow.Width)])));
                //                             /\     Código serial lento! Odeio você!     /\
                //======================================================================================================


                pictureBox1.Image = toShow;
                this.renderScene();
            }
        }
    }
}