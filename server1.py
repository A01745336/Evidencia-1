from flask import Flask, request, jsonify
from AcomodarCajasModel1 import *

# Size of the board:
number_agents = 5
width = 28
height = 28
randomModel = None
currentStep = 0
cajas = 150
pasosMaximos = 20000

app = Flask("Traffic example")

# @app.route('/', methods=['POST', 'GET'])


@app.route('/init', methods=['POST', 'GET'])
def initModel():
    global currentStep, randomModel, number_agents, width, height

    if request.method == 'POST':
        number_agents = int(request.form.get('NAgents'))
        width = int(request.form.get('width'))
        height = int(request.form.get('height'))
        cajas = int(request.form.get('cajas'))
        pasos = int(request.form.get('pasosMaximos'))
        currentStep = 0

        print(request.form)
        print(number_agents, width, height)
        randomModel = AlmacenModel(number_agents, width, height, cajas, pasos)

        return jsonify({"message": "Parameters recieved, model initiated."})


@app.route('/getAgents', methods=['GET'])
def getAgents():
    global randomModel

    if request.method == 'GET':
        agentPositions = [{"id": str(a.unique_id), "x": x, "y": 1, "z": z} for
                          (a, x, z) in randomModel.grid.coord_iter() if
                          isinstance(a, Walle)]

        return jsonify({'positions': agentPositions})


@app.route('/getCajas', methods=['GET'])
def getCajas():
    global randomModel

    if request.method == 'GET':
        boxPosition = [{"id": str(agent.unique_id), "x": x, "y": 1, "z": z,
                       "tipo": agent.tipo} for (a, x, z) in
                       randomModel.grid.coord_iter() for agent in a if
                       isinstance(agent, Cajas)]

        return jsonify({'positions': boxPosition})


@app.route('/getPilas', methods=['GET'])
def getPilas():
    global randomModel

    if request.method == 'GET':
        pilaPosition = [{"id": str(agent.unique_id), "x": x, "y": 1, "z": z,
                         "numCajas": agent.numCajas} for (a, x, z) in
                        randomModel.grid.coord_iter() for agent in a if
                        isinstance(agent, Repisas)]

        return jsonify({'positions': pilaPosition})


@app.route('/update', methods=['GET'])
def updateModel():
    global currentStep, randomModel
    if request.method == 'GET':
        randomModel.step()
        currentStep += 1
        return jsonify({'message': f'Model updated to step {currentStep}.',
                       'currentStep': currentStep})


if __name__ == '__main__':
    app.run(host="localhost", port=8521, debug=True)
