from AcomodarCajasModel1 import *
from flask import Flask, request, jsonify
import numpy as np

def updatePositionsWalle(flock):
    positionWalle = []
    for Walle in flock:
        Walle.apply_behaviour(flock)
        Walle.update()
        Walle.edges()
        positionWalle.append((Walle.id, Walle.position))
    return positionWalle


def updatePositionsCaja(flock):
    positionCaja = []
    for caja in flock:
        caja.apply_behaviour(flock)
        caja.update()
        caja.edges()
        positionCaja.append((caja.id, caja.position))
    return positionCaja

def updatePositionsRepisa(flock):
    positionRepisa = []
    for repisa in flock:
        repisa.apply_behaviour(flock)
        repisa.update()
        repisa.edges()
        positionRepisa.append((repisa.id, repisa.position))

    return positionRepisa

def positionsWalleToJSON(positions):
    posDICT = []
    for id, p in positions:
        pos = {
            "walleId" : str(id),
            "x" : float(p.x),
            "y" : float(p.z),
            "z" : float(p.y)
        }
        posDICT.append(pos)
    return jsonify({'positions':posDICT})

def positionsCajaToJSON(positions):
    posDICT = []
    for id, p in positions:
        pos = {
            "cajaId" : str(id),
            "x" : float(p.x),
            "y" : float(p.z),
            "z" : float(p.y)
        }
        posDICT.append(pos)
    return jsonify({'positions':posDICT})

def positionsRepisaToJSON(positions):
    posDICT = []
    for id, p in positions:
        pos = {
            "repisaId" : str(id),
            "x" : float(p.x),
            "y" : float(p.z),
            "z" : float(p.y)
        }
        posDICT.append(pos)
    return jsonify({'positions':posDICT})

flock = []
app = Flask("Integradora")


@app.route('/', methods=['POST', 'GET'])
def posicionWalle():
    if request.method == 'GET':
        positions = updatePositionsWalle(flock)
        return positionsWalleToJSON(positions)
    elif request.method == 'POST':
        return "Post request from Boids example\n"


@app.route('/init', methods=['POST', 'GET'])
def WalleInit():
    global flock
    if request.method == 'GET':
        # Set the number of agents here:
        flock = [Walle(*np.random.rand(2)*30, 30, 30, id) for id in range(20)]
        return jsonify({"num_agents":5, "w": 30, "h": 30})
    elif request.method == 'POST':
        return "Post request from init\n"


if __name__=='__main__':
    app.run(host="localhost", port=8521, debug=True)

