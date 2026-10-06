namespace Domain.Reportes;

public record TipoDanoCatalogo(TipoDano Tipo, string Nombre);

public record CategoriaDanoCatalogo(CategoriaDano Categoria, string Nombre, IReadOnlyList<TipoDanoCatalogo> Tipos);

// Fuente única de las categorías de daño, sus tipos y los nombres que ve el usuario.
public static class CatalogoDanos
{
    public static IReadOnlyList<CategoriaDanoCatalogo> Categorias { get; } =
    [
        new(CategoriaDano.ViasYAndenes, "Vías y andenes",
        [
            new(TipoDano.HuecosEnLaVia, "Huecos en la vía"),
            new(TipoDano.HundimientoDelPavimento, "Hundimiento del pavimento"),
            new(TipoDano.GrietasEnLaVia, "Grietas en la vía"),
            new(TipoDano.AndenRoto, "Andén roto"),
            new(TipoDano.AndenLevantadoPorRaices, "Andén levantado por raíces"),
            new(TipoDano.CiclorrutaDanada, "Ciclorruta dañada"),
            new(TipoDano.PuentePeatonalEnMalEstado, "Puente peatonal en mal estado"),
            new(TipoDano.ReductorDeVelocidadDanado, "Reductor de velocidad dañado")
        ]),
        new(CategoriaDano.Senalizacion, "Señalización",
        [
            new(TipoDano.SemaforoApagado, "Semáforo apagado"),
            new(TipoDano.SemaforoDanado, "Semáforo dañado"),
            new(TipoDano.SenalDeTransitoCaida, "Señal de tránsito caída"),
            new(TipoDano.SenalIlegibleOTapada, "Señal ilegible o tapada"),
            new(TipoDano.DemarcacionVialBorrada, "Demarcación vial borrada"),
            new(TipoDano.PasoDeCebraBorrado, "Paso de cebra borrado")
        ]),
        new(CategoriaDano.AlumbradoYElectricidad, "Alumbrado y electricidad",
        [
            new(TipoDano.LuminariaApagada, "Luminaria apagada"),
            new(TipoDano.LuminariaIntermitente, "Luminaria intermitente"),
            new(TipoDano.PosteCaidoOInclinado, "Poste caído o inclinado"),
            new(TipoDano.CablesSueltosOExpuestos, "Cables sueltos o expuestos"),
            new(TipoDano.CajaElectricaAbierta, "Caja eléctrica abierta")
        ]),
        new(CategoriaDano.AguaYAlcantarillado, "Agua y alcantarillado",
        [
            new(TipoDano.TapaDeAlcantarillaFaltante, "Tapa de alcantarilla faltante"),
            new(TipoDano.TapaDeAlcantarillaRota, "Tapa de alcantarilla rota"),
            new(TipoDano.AlcantarillaTapada, "Alcantarilla tapada"),
            new(TipoDano.SumideroObstruido, "Sumidero obstruido"),
            new(TipoDano.FugaDeAgua, "Fuga de agua"),
            new(TipoDano.Encharcamiento, "Encharcamiento"),
            new(TipoDano.Inundacion, "Inundación"),
            new(TipoDano.MalosOlores, "Malos olores")
        ]),
        new(CategoriaDano.ParquesYZonasVerdes, "Parques y zonas verdes",
        [
            new(TipoDano.ArbolCaido, "Árbol caído"),
            new(TipoDano.ArbolEnRiesgoDeCaida, "Árbol en riesgo de caída"),
            new(TipoDano.RamasPeligrosas, "Ramas peligrosas"),
            new(TipoDano.PastoOMalezaAlta, "Pasto o maleza alta"),
            new(TipoDano.JuegoInfantilDanado, "Juego infantil dañado"),
            new(TipoDano.GimnasioAlAireLibreDanado, "Gimnasio al aire libre dañado")
        ]),
        new(CategoriaDano.MobiliarioUrbano, "Mobiliario urbano",
        [
            new(TipoDano.BancaRota, "Banca rota"),
            new(TipoDano.CanecaDeBasuraDanada, "Caneca de basura dañada"),
            new(TipoDano.CanecaFaltante, "Caneca faltante"),
            new(TipoDano.ParaderoDeBusDanado, "Paradero de bus dañado"),
            new(TipoDano.BarandaORejaDanada, "Baranda o reja dañada"),
            new(TipoDano.BebederoAveriado, "Bebedero averiado")
        ]),
        new(CategoriaDano.Aseo, "Aseo",
        [
            new(TipoDano.BasuraAcumulada, "Basura acumulada"),
            new(TipoDano.EscombrosEnLaVia, "Escombros en la vía"),
            new(TipoDano.AnimalMuertoEnLaVia, "Animal muerto en la vía"),
            new(TipoDano.ContenedorDesbordado, "Contenedor desbordado")
        ]),
        new(CategoriaDano.VandalismoYEdificiosPublicos, "Vandalismo y edificios públicos",
        [
            new(TipoDano.Grafitis, "Grafitis"),
            new(TipoDano.FachadaDeteriorada, "Fachada deteriorada"),
            new(TipoDano.MuroOCerramientoDanado, "Muro o cerramiento dañado"),
            new(TipoDano.BanoPublicoDanado, "Baño público dañado")
        ]),
        new(CategoriaDano.Accesibilidad, "Accesibilidad",
        [
            new(TipoDano.RampaDanadaOFaltante, "Rampa dañada o faltante"),
            new(TipoDano.ObstaculoEnElAnden, "Obstáculo en el andén"),
            new(TipoDano.GuiaTactilDanada, "Guía táctil dañada")
        ]),
        new(CategoriaDano.Otros, "Otros",
        [
            new(TipoDano.Otros, "Otros")
        ])
    ];

    private static readonly Dictionary<TipoDano, CategoriaDano> CategoriaPorTipo = Categorias
        .SelectMany(categoria => categoria.Tipos.Select(tipo => (tipo.Tipo, categoria.Categoria)))
        .ToDictionary(par => par.Tipo, par => par.Categoria);

    public static bool PerteneceA(TipoDano tipo, CategoriaDano categoria) =>
        CategoriaPorTipo.TryGetValue(tipo, out CategoriaDano categoriaDelTipo) && categoriaDelTipo == categoria;
}
