#ifndef __DCDZ_CALLBACKS_H__
#define __DCDZ_CALLBACKS_H__

#include "Neuron.h"
namespace DeepCDZ
{
	//Neuron factory
	typedef Neuron* (*Callback_NeuronFactory)();

	//Numerical functions
	typedef double(*Callback_1DDouble_1DDouble)(double);
	typedef double(*Callback_2DDouble_1DDouble)(double, double);
	typedef double(*Callback_2DInt_1DDouble)(int, int);

	typedef double(*Callback_8DInt_1DDouble)(int, int, int, int, int, int, int, int);
}
#endif