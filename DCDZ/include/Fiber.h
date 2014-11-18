#ifndef __DCDZ_FIBER_H__
#define __DCDZ_FIBER_H__

#include "Sheet.h"

namespace DeepCDZ
{


	//An empty unimodal layered structure of neural sheets.
	class Fiber: private Pool
	{
	private:
		std::vector< Sheet* > layers;

	public:

		enum InterlayerTopology { Random, All2All, TopographicalRectangular, TopographicalToroidal };

		//------------------------------
		//Create an empty multi layered structure of 2D sheets
		Fiber();

		//------------------------------
		//Create a multi layered structure of 2D sheets
		//@param structure Dimension of each layer.
		//@param factory the factory of neuron to be used
		Fiber(std::vector<int> structure, Callback_NeuronFactory factory);

		//------------------------------
		//Add a new layer to the fiber
		//@param structure Dimension of each layer.
		//@param factory the factory of neuron to be used
		void addLayer(int width, int height, Callback_NeuronFactory factory);


		//------------------------------
		//Update all the layers, starting from 0 to the last
		//@param should perform rnd order update of neurons in every specific layer
		virtual void update(bool rndOrder = false);

		//------------------------------
		//Connect 2 layers together, using a specific pattern
		//@param layer1 index of the source layer
		//@param layer2 index of the target layer
		//@param topology topology pattern to be used
		//@param density (connection probability in the case of Random, project radius in the case of topographical, doesn't matter for all2all)
		//@param weightFx A callback to a function defining weight according to distance. We assume that the neurons are evenly distributed on a sheet dimension of 1.0 by 1.0 
		bool connectLayers(unsigned int layer1, unsigned int layer2, InterlayerTopology topology, double density, Callback_1DDouble_1DDouble weightFx=NULL);
	};
}

#endif

