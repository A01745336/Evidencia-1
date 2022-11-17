from AcomodarCajasModel import *
from mesa.visualization.modules import ChartModule
from mesa.visualization.modules import CanvasGrid
from mesa.visualization.ModularVisualization import ModularServer


def agent_portayal(agent):
    """
    It returns a dictionary that describes how the agent should be drawn

    :param agent: the agent to be portrayed
    :return: a dictionary with the color, shape, radius and layer of the agent.
    """
    portayal = {"Filled": "true"}

    if (type(agent) == Walle):
        portayal["Color"] = "green"
        portayal["Shape"] = "circle"
        portayal["r"] = 0.5
        portayal["Layer"] = 2

    elif (type(agent) == Cajas):
        portayal["Color"] = "brown"
        portayal["Shape"] = "circle"
        portayal["r"] = 0.5
        portayal["Layer"] = 1

    else:
        portayal["Shape"] = "rect"
        portayal["Color"] = "blue"
        portayal["w"] = 1
        portayal["h"] = 1
        portayal["Layer"] = 0

    return portayal


grid = CanvasGrid(agent_portayal, 10, 10, 500, 500)

movimientos = ChartModule(
    [{
        "Label": "Movimientos",
        "Color": "Black"
    }],
    data_collector_name='datacollector'
)

tiempo = ChartModule(
    [{
        "Label": "Tiempo",
        "Color": "Green"
    }],
    data_collector_name='datacollector'
)

server = ModularServer(AlmacenModel,
                       [grid, tiempo, movimientos],
                       "Almacen Model",
                       {"N": 5, "width": 10, "height": 10, "numCajas": 50,
                        "time": 580})
server.port = 8521
server.launch()
