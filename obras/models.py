from django.db import models


class Obra(models.Model):

    ESTADO = [('asignada', 'asignada'),('iniciada', 'iniciada'),
              ('finalizada','finalizada'),('entregada','entregada')]

    nombre = models.CharField(max_length=200)
    descripcion = models.TextField(blank=True)
    direccion = models.CharField(max_length=250)
    entidad_responsable = models.CharField(max_length=200)
    estado = models.CharField(max_length=20,choices=ESTADO,default="asignada")
    avance = models.PositiveIntegerField(default=0)

    fecha_creacion = models.DateTimeField(auto_now_add=True)

    def __str__(self):
        return self.nombre