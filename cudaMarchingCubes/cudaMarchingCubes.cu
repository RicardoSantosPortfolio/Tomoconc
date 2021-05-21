#include <cuda.h>
#include <cuda_runtime.h>
#include <stdio.h>
#include <string.h>


//Declarando a estrutura que recebe o ponto com coordenadas e cor
struct pontoXYZcor
{
	float x, y, z;
	int RGBa;
};

__global__ void marchingCubes(int* entrada, int* saida, int width, int height, int threshold)
{
	/*Aqui, cada coluna é executada por uma thread e cada linha por um bloco, então:

	bloco 0, thread 0 executa: img[0][0];
	bloco 0, thread 1 executa: img[0][1];
	bloco 0, thread 2 executa: img[0][2];

	...

	bloco n, thread 0 executa: img[n][0];
	bloco n, thread 1 executa: img[n][1];
	bloco n, thread 2 executa: img[n][2];

	...

	bloco n, thread m-2 executa: img[n][m-2];
	bloco n, thread m-1 executa: img[n][m-1];
	bloco n, thread m executa: img[n][m].

	Sendo m o número de colunas e n o número de linhas da imagem

	*/
	int i = threadIdx.x + blockDim.x * blockIdx.x;

	if(i < width * height)
	{
		int R = (entrada[i] & 0x00FFFFFF) >> 16;
		int G = (entrada[i] & 0x0000FFFF) >> 8;
		int B = (entrada[i] & 0x000000FF);
		int gs = ((R)*0.3)+((G)*0.59)+((B)*0.11);

		if ( gs >= threshold )
			saida[i] = entrada[i];
		else
			saida[i] = 0xFF000000;
	}

	return;
}

extern "C"
{

	__declspec(dllexport) int* cudaMarchingCubes(int* entrada, int width, int height, int threshold)
	{
		int* i;

		//Declarando as variáveis do device
		int *d_entrada, *d_saida;
		
		i = (int*)malloc(width*height*sizeof(int));

		//Alocando as variáveis do device
		cudaMalloc((void**)&d_entrada, width*height*sizeof(int));
		cudaMalloc((void**)&d_saida, width*height*sizeof(int));

		//Inicializar variáveis CUDA
		cudaMemcpy(d_entrada, entrada, width*height*sizeof(int), cudaMemcpyHostToDevice);

		marchingCubes<<<width,height>>>(d_entrada, d_saida, width, height, threshold);

		//Copiar retorno das variáveis CUDA
		cudaMemcpy(i, d_saida, width*height*sizeof(int), cudaMemcpyDeviceToHost);

		//Liberar as variáveis CUDA
		cudaFree(d_entrada);
		cudaFree(d_saida);

		cudaDeviceReset();

		//Retornar o resultado do processamento
		//memcpy(saida, i, sizeof(i));
		return i;
	}

}

