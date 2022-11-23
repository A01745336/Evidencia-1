from flask import Flask, request, jsonify
from AcomodarCajasModel import *

# Size of the board:
number_agents = 1
numero_cajas = 50
tiempo = 0
width = 28
height = 28
almacenModel = None
currentStep = 0

app = Flask("Acomodar Cajas Model")


@app.route('/init', methods=['POST', 'GET'])
def initModel():
    global currentStep, almacenModel, number_agents, width, height

    if request.method == 'POST':
        number_agents = int(request.form.get('NAgents'))
        width = int(request.form.get('width'))
        height = int(request.form.get('height'))
        currentStep = 0

        print(request.form)
        print(number_agents, width, height)
        almacenModel = AlmacenModel(number_agents, width, height,
                                    numero_cajas, tiempo)

        return jsonify({"message": "Parameters recieved, model initiated."})


@app.route('/getWalle', methods=['GET'])
def getWalle():
    global almacenModel

    if request.method == 'GET':
        agentPositions = [{"id": str(a.unique_id), "x": x, "y": 0.1, "z": z}
                          for (a_, x, z) in almacenModel.grid.coord_iter()
                          for a in a_ if isinstance(a, Walle)]

        return jsonify({'positions': agentPositions})


@app.route('/getCajas', methods=['GET'])
def getObstacles():
    global almacenModel

    if request.method == 'GET':
        cajaPositions = [{"id": str(a.unique_id), "x": x, "y": 0.1, "z": z}
                         for (a_, x, z) in almacenModel.grid.coord_iter()
                         for a in a_ if isinstance(a, Cajas)]

        return jsonify({'positions': cajaPositions})


@app.route('/getRepisas', methods=['GET'])
def getRepisas():
    global almacenModel

    if request.method == 'GET':
        repisaPositions = [{"id": str(a.unique_id), "x": x, "y": 0.1, "z": z}
                           for (a_, x, z) in almacenModel.grid.coord_iter()
                           for a in a_ if isinstance(a, Repisas)]

        return jsonify({'positions': repisaPositions})


@app.route('/update', methods=['GET'])
def updateModel():
    global currentStep, almacenModel
    if request.method == 'GET':
        almacenModel.step()
        currentStep += 1
        return jsonify({'message': f'Model updated to step {currentStep}.',
                        'currentStep': currentStep})


if __name__ == '__main__':
    app.run(host="localhost", port=8585, debug=True)
