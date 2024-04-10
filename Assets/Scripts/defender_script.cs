using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class defender_script : MonoBehaviour
{

    class RoleAttributes
    {
        public enum Role
        {
            Mage, Healer, Tank, SharpShooter
        }
        Role role;

        public int physical_defence { get; }
        public int magic_defence { get; }
        public int physical_penetration { get; }
        public int magic_penetration { get; }

        public int damage { get; }
        public int heal_amount { get; }
        Action[] allowedActions { get; }
        public string damage_type { get; }
        RoleAction roleAction;
        public Color meshColor;

        public RoleAttributes(Role role)
        {
            this.role = role;
            (int, int, int, int, int, int, string, RoleAction, Color) attrs = this.role switch
            {
                Role.Mage =>            (0,  10, 0,  20, 10, 30, "magical", DebuffEnemies, Color.blue),
                Role.Healer =>          (0,  5,  0,  10, 8,  75, "magical", TotalPartyHeal, Color.green),
                Role.Tank =>            (25, 0,  5,  2,  5,  5,  "physical", Cannon, Color.black),
                Role.SharpShooter =>    (15, 0,  20, 0,  10, 5,  "physical", ClearLane, Color.red),
                _ =>                    (-1, -1, -1, -1, -1, -1, "", NoRoleAction, Color.grey),
            };
            this.physical_defence = attrs.Item1;
            this.magic_defence = attrs.Item2;
            this.physical_penetration = attrs.Item3;
            this.magic_penetration = attrs.Item4;
            this.damage = attrs.Item5;
            this.heal_amount = attrs.Item6;
            this.damage_type = attrs.Item7;
            this.roleAction = attrs.Item8;
            this.meshColor = attrs.Item9;
        }

        void NoRoleAction(defender_script _) { }

        void Cannon(defender_script defenderView)
        {

        }

        void TotalPartyHeal(defender_script defenderView)
        {

        }

        void ClearLane(defender_script defenderView)
        {

        }

        void DebuffEnemies(defender_script defenderView)
        {

        }

        // Cannon,
        // T.P. Heal
        // Headshot
        // Remove Armor

    }

    delegate void RoleAction(defender_script defenderView);

    private RoleAttributes roleAttributes;

    [SerializeField]
    RoleAttributes.Role Role;

    //Store location in x,y,z
    public int x;
    private const int y = 0;
    public int z;
    private const int max_life = 100;
    public int life;
    private const int damage = 1;
    private const int max_energy = 10000;
    private const int start_energy = 1000;

    private const int energy_refill_rate = 10;

    private int energy;

    public attacker_script attacker;
    System.Random random = new System.Random();
    // Start is called before the first frame update
    void Start()
    {
        roleAttributes = new RoleAttributes(this.Role);

        GetComponent<Renderer>().material.SetColor("_Color", roleAttributes.meshColor);

        //Initialize variables
        x = random.Next(10);
        life = max_life;
        energy = start_energy;

        update_graphic();
    }

    // Update is called once per frame
    void Update()
    {
        take_action();
        increase_energy();
        update_graphic();
    }
    
    //Calculate an action based on the defender's known information and take that action
    void take_action() {
        int[] known_information = get_known_information();
        int action = get_action(known_information);
        switch(action) {
            case 0:
                move_left();
                break;
            case 1:
                move_right();
                break;
            case 2:
                shoot();
                break;
            case 3:
                heal();
                break;
            case 4:
                do_nothing();
                break;
        }
    }

    //Gather the information known to the defender and format it for processing
    int[] get_known_information() {
        return null;
    }

    //Decides on an action based on the passed information
    int get_action(int[] known_information) {
        return random.Next(5);
    }

    //Move to the square to the left if it is available
    void move_left() {
        //Checks that the action is affordable; otherwise does nothing this tick
        if(!check_energy(10)) return;
        print_action("move_left");
        x -= 1;
        x = Math.Max(0, x);
    }

    //Move to the square to the right if it is available
    void move_right() {
        //Checks that the action is affordable; otherwise does nothing this tick
        if(!check_energy(10)) return;

        print_action("move_right");
        x += 1;
        x = Math.Min(x, 9);
    }

    //Shoot the closest spawn in the same lane if one is there
    void shoot() {
        //Checks that the action is affordable; otherwise does nothing this tick
        if(!check_energy(50)) return;

        print_action("shoot");
        ArrayList spawns = attacker.spawns;
        spawn_script closest_spawn = null;
        foreach(spawn_script spawn in spawns) {
            if(spawn.x == x && spawn.z >= z && (closest_spawn == null || spawn.z < closest_spawn.z)) {
                closest_spawn = spawn;
            }
        }
        if(closest_spawn != null)
            closest_spawn.take_damage(damage, spawns, roleAttributes.physical_penetration, roleAttributes.magic_penetration, roleAttributes.damage_type);
    }

    //Recover health
    void heal() {
        //Checks that the action is affordable; otherwise does nothing this tick
        if(!check_energy(100)) return;

        print_action("heal");
        life += roleAttributes.heal_amount;
        life = Math.Min(life, max_life);
    }

    //To assist in energy management, sometimes does nothing
    void do_nothing() {
        print_action("do_nothing");
    }

    //Determines if energy is high enough for the intended action
    //Decrements energy if possible
    bool check_energy(int cost) {
        print("A: " + z + " | E: " + energy);
        if(energy < cost) {
            print_action("cannot_aford_action | Costs: " + cost + " of " + energy);
            return false;
        } else {
            energy -= cost;
            return true;
        }
    }

    //Update's the defender's visual representation
    void update_graphic() {
        transform.position = new Vector3(x, y, z);

    }

    //Increases the defender's action each tick
    void increase_energy() {
        energy = Math.Min(max_energy, energy + energy_refill_rate);
    }

    //Prints out an action in a formatted manner
    void print_action(string action) {
        //print("Unit: Defender " + z + " || Action: " + action);  
    }

    /* Take damage dealt by a spawn, and accept defeat if life is reduced to 0
    ** damage_dealt: the original damage dealt by a defender
    ** spawns: the list of spawns, which this spawn may be removing itself from
    ** physical_penetration: the ammount of physical_defense that is ignored by the attack
    ** magic_penetration: the ammount of magic_defense that is ignored by the attack
    ** damage_type: the type of damage, either physical or magic, which corresponds to defense and penetration values
    */
    public void take_damage(int damage_dealt, int physical_penetration, int magic_penetration, string damage_type) {
        int old_life = life;
        int total_damage = damage_dealt;

        //decrease damage by any defense of the damage type, after reducing defense by penetration
        int total_physical_penetration = Math.Max(0, physical_penetration - roleAttributes.physical_defence);
        int total_magic_penetration = Math.Max(0, magic_penetration - roleAttributes.magic_defence);
        if(damage_type == "physical") {
            total_damage = Math.Max(0, total_damage - total_physical_penetration);
        } else if(damage_type == "magical") {
            total_damage = Math.Max(0, total_damage - total_magic_penetration);
        }

        //Reduce life by the calculated damage
        life -= total_damage;
        if(life <= 0 && old_life > 0) {
            print("GAME OVER, DEFEATED DEFENDER");
        }
    }
}