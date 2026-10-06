from rest_framework import serializers

from .models import Obra


class ObraSerializer(serializers.ModelSerializer):
    class Meta:
        model = Obra
        fields = "__all__"

    def validate_avance(self, value):
        if value <= 0 or value >= 100:
            raise serializers.ValidationError("El avance debe estar entre 0 y 100")
        return value