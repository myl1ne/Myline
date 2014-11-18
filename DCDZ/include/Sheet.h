#ifndef __DCDZ_SHEET_H__
#define __DCDZ_SHEET_H__

#include "Pool.h"
#include <vector>

namespace DeepCDZ
{

	//A 2 dimensional arrangment of neurons
	class Sheet: public Pool
	{
	
	private:
		int width, height;
		std::vector< std::vector< Neuron* > > neurons2D;

	public:

		enum Topology { Rectangular, Toroidal };

		//------------------------------
		//Create a 2D sheet of neurons
		//@param _w Width of the sheet
		//@param _h Height of the sheet
		//@param factory the factory of neuron to be used
		Sheet(int _w, int _h, Callback_NeuronFactory factory);
		
		//------------------------------
		//Getter for the height of the sheet
		int Height() { return height; }

		//------------------------------
		//Getter for the width of the sheet
		int Width() { return width; }

		//------------------------------
		//Access a neuron at given coordinates
		//@param x X coordinate of the neuron to retrieve
		//@param y Y coordinate of the neuron to retrieve
		Neuron* neuron(int x, int y) { return neurons2D[x][y]; };

		//------------------------------
		//Create lateral connections between neurons of the sheet
		//@param radius Radius of connectivity
		//@param topology Topology used (Rectangular or Toroidal)
		//@param weightFx How weights are assigned based on distance (NULL means rnd() in [0,1] )
		void CreateLateralConnections(int radius, Topology topology, Callback_1DDouble_1DDouble weightFx=NULL);

	};
}

#endif