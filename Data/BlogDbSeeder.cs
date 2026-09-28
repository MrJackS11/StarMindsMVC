using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Models;

namespace StarMindsMVC.Data;

public static class BlogDbSeeder
{
    public static async Task SeedArticulosAsync(StarMindsContext context)
    {
        try
        {
            // Log de usuarios registrados para diagnóstico
            var usuariosRegistrados = await context.Usuarios.Select(u => new { u.Id, u.Correo, u.Activo, u.RolId }).ToListAsync();
            foreach (var u in usuariosRegistrados)
            {
                Console.WriteLine($"[DB_USER] Id={u.Id}, Correo='{u.Correo}', Activo={u.Activo}, RolId={u.RolId}");
            }
            var autorId = usuariosRegistrados.FirstOrDefault()?.Id ?? 1;

            bool huboCambios = false;

            // 1. Artículo sobre Depresión
            if (!await context.ArticulosBlog.AnyAsync(a => a.Categoria == "Depresión" || a.Titulo.Contains("Depresión")))
            {
                context.ArticulosBlog.Add(new ArticuloBlog
                {
                    Titulo = "Superar la Apatía y Romper el Ciclo de la Depresión Universitaria",
                    Categoria = "Depresión",
                    AutorId = autorId,
                    FechaPublicacion = DateTime.Now,
                    Contenido = @"<p class='lead'>La depresión en la universidad rara vez se manifiesta solo como tristeza; con frecuencia se disfraza de apatía profunda, agotamiento crónico, procrastinación involuntaria y desconexión de los estudios.</p>

<h3>¿Qué es el ciclo de la inactividad y cómo atraparlo?</h3>
<p>Cuando el estado de ánimo decae, el cerebro envía señales para aislarse y evitar el esfuerzo. Sin embargo, al reducir nuestras actividades agradables y académicas, disminuyen también las fuentes de recompensa y dopamina, profundizando el malestar.</p>

<div class='p-3 my-3 rounded-3' style='background-color: #F8F9FA; border-left: 4px solid #2A6F97;'>
    <strong>Estrategia clínica clave: Activación Conductual</strong>
    <p class='mb-0 small text-muted'>La acción precede a la motivación. No esperes a tener ganas para comenzar; realiza micro-acciones de 5 minutos para que la motivación aparezca después de haber iniciado.</p>
</div>

<h3>Pautas cotidianas para recuperar el bienestar</h3>
<ul>
    <li><strong>Regla de los micro-objetivos:</strong> Divide las lecturas y proyectos pesados en bloques de 15 minutos con descansos reales.</li>
    <li><strong>Higiene de la luz y el movimiento:</strong> Busca 15 minutos de sol matutino y una caminata breve diaria para estimular la regulación circadiana.</li>
    <li><strong>Rompe el aislamiento:</strong> Aunque no desees hablar en profundidad, mantén contacto presencial o comparte una mesa de estudio en la biblioteca con compañeros.</li>
    <li><strong>Cuestiona la autocrítica:</strong> Reconoce que la apatía no es falta de voluntad ni pereza, sino un síntoma clínico tratable.</li>
</ul>

<p>Recuerda que no tienes que enfrentar esto en soledad. Solicitar una sesión de orientación psicológica en Star Minds te permite contar con un espacio seguro, anónimo y guiado por profesionales.</p>"
                });
                huboCambios = true;
            }

            // 2. Artículo sobre Apego
            if (!await context.ArticulosBlog.AnyAsync(a => a.Categoria == "Apego" || a.Titulo.Contains("Apego")))
            {
                context.ArticulosBlog.Add(new ArticuloBlog
                {
                    Titulo = "Estilos de Apego y Vínculos Sanos en la Vida Universitaria",
                    Categoria = "Apego",
                    AutorId = autorId,
                    FechaPublicacion = DateTime.Now,
                    Contenido = @"<p class='lead'>Nuestras formas de relacionarnos con amigos, parejas y equipos de trabajo en la universidad están profundamente influenciadas por nuestro estilo de apego.</p>

<h3>Los cuatro estilos de apego y su manifestación en la universidad</h3>
<ul>
    <li><strong>Apego Seguro:</strong> Confianza en uno mismo y en los demás. Capacidad para pedir ayuda a compañeros y docentes sin temor al rechazo.</li>
    <li><strong>Apego Ansioso:</strong> Hipervigilancia ante la aprobación ajena, miedo constante al abandono o exclusión de grupos sociales, y necesidad excesiva de validación en trabajos en equipo.</li>
    <li><strong>Apego Evitativo:</strong> Autosuficiencia defensiva. Tendencia a aislarse, rechazar la ayuda ajena y desestimar las emociones para no sentirse vulnerable.</li>
    <li><strong>Apego Desorganizado:</strong> Oscilación entre el deseo de cercanía afectiva y el temor o huida cuando las relaciones se tornan más íntimas.</li>
</ul>

<div class='p-3 my-3 rounded-3' style='background-color: #F8F9FA; border-left: 4px solid #528788;'>
    <strong>¿Cómo construir un apego seguro adquirido?</strong>
    <p class='mb-0 small text-muted'>El estilo de apego no es una condena fija. Mediante el autoconocimiento y la psicoterapia es posible transitar hacia vínculos más estables y confiados.</p>
</div>

<h3>3 Claves para cultivar relaciones saludables</h3>
<ol>
    <li><strong>Comunicación asertiva de necesidades:</strong> Expresa lo que sientes sin recurrir al silencio defensivo ni al reproche impulsivo.</li>
    <li><strong>Establecimiento de límites sin culpa:</strong> Decir 'no' a planes o exigencias excesivas protege tu salud emocional y no deteriora los vínculos genuinos.</li>
    <li><strong>Diferenciar dependencia de reciprocidad:</strong> Un vínculo sano nutre la autonomía de cada persona en lugar de limitarla.</li>
</ol>

<p>Si sientes que tus patrones relacionales te causan sufrimiento repetitivo, el equipo de Star Minds está listo para acompañarte en un proceso de orientación personalizada.</p>"
                });
                huboCambios = true;
            }

            if (huboCambios)
            {
                await context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BlogDbSeeder] Nota: {ex.Message}");
        }
    }
}
