using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicScore;

namespace LevelClasses {
    public class LevelBuilder {

        // Gera todo "Level" associado ao "Score"
        public static Level[] Batch(Score score) {
            List<Level> batch = new List<Level>();
            List<Scale> order;
            Level newLevel;
            int pc = 0;

            foreach (Part p in score.Parts) {
                newLevel = new Level();
                newLevel.PartID = pc++;
                order = newLevel.orderRoles(p);

                // Validação de Instrumento
                if (order.Count < 7) {
                    continue;
                }

                newLevel.SingleLoop(p, order);
                batch.Add(newLevel);
            }

            return batch.ToArray();
        }
    }
}
