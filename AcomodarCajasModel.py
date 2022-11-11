import mesa


class Walle(mesa.Agent):
    def __init__(self, unique_id: int, model: mesa.Model) -> None:
        super().__init__(unique_id, model)
        self.movimientos = 0
        self.carga = False

    def cargar(self, cell) -> None:
        self.carga = True

    def mover(self) -> None:
        self.movimientos += 1
        posibleMovimiento = self.model.grid.get_neighborhood(
            self.pos,
            moore=True,
            include_center=False,
            include_diagonals=False)

        nuevaPosicion = self.random.choice(posibleMovimiento)
        self.model.grid.move_agent(self, nuevaPosicion)

    def stpe(self) -> None:
        if (self.pos in self.model.mess):
            self.cargar(self.pos)

        else:
            self.mover()


class Cajas(mesa.Agent):
    ...


class AlmacenModel(mesa.Model):
    ...
